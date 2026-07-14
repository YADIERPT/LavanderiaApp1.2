# Reporte Exhaustivo de Auditoría: Elementos Estáticos, Valores Hardcodeados, Fallos de Lógica y Contradicciones

**Proyecto:** LavanderiaApp0.1  
**Ubicación del reporte:** `C:\Users\Yadie\RiderProjects\LavanderiaApp0.1\App\Shared\REPORTE_AUDITORIA_ESTATICA_Y_LOGICA.md`  
**Fecha de Revisión:** 13 de Julio de 2026  
**Alcance:** Revisión exhaustiva de todos los archivos del proyecto (`Modelos`, `BaseDatos`, `Repositorios`, `Servicios`, `Pages`, `InterfazUsuario`, `Shared`, scripts auxiliares).  
**Nota:** De acuerdo con la instrucción, no se ha modificado ningún archivo de código del proyecto; este informe documenta de forma minuciosa todos los hallazgos con rutas de archivo y números de línea exactos.

---

## ÍNDICE DE HALLAZGOS

1. [Arquitectura y Estado Global Estático (Anti-patrones DI)](#1-arquitectura-y-estado-global-estático-anti-patrones-di)
2. [Valores Hardcodeados y Datos Ficticios en Negocio / UI](#2-valores-hardcodeados-y-datos-ficticios-en-negocio--ui)
3. [Rutas Absolutas Hardcodeadas dependientes de la Máquina Local](#3-rutas-absolutas-hardcodeadas-dependientes-de-la-máquina-local)
4. [Fallos y Contradicciones Lógicas Críticas entre Módulos](#4-fallos-y-contradicciones-lógicas-críticas-entre-módulos)
5. [Errores de Concurrencia, Gestión de Transacciones y Excepciones Silenciadas](#5-errores-de-concurrencia-gestión-de-transacciones-y-excepciones-silenciadas)
6. [Matriz de Resumen y Prioridad de Corrección](#6-matriz-de-resumen-y-prioridad-de-corrección)

---

## 1. ARQUITECTURA Y ESTADO GLOBAL ESTÁTICO (ANTI-PATRONES DI)

En una aplicación Blazor Hybrid con Inyección de Dependencias (DI), el uso indiscriminado de miembros y clases `static` acopla componentes, impide pruebas unitarias y genera problemas de aislamiento y ciclo de vida.

### 1.1 Propiedad Estática `SharedMachines` en Componente UI (`Maquinas.razor`)
* **Archivo:** `App/Pages/Maquinas.razor` (Línea 256)
* **Código:**
  ```csharp
  public static List<MachineItem> SharedMachines
  {
      get
      {
          try
          {
              var repo = new MaquinaRepositorio();
              ...
          }
      }
  }
  ```
* **Problema:** Un componente visual (`Maquinas.razor`) expone una propiedad `public static` que es consumida directamente por `Dashboard.razor` (Línea 331) y `MainLayout.razor` (Línea 641).
* **Impacto:**
  1. Cada vez que `Dashboard` o `MainLayout` leen `SharedMachines`, se instancia `new MaquinaRepositorio()` y se ejecuta una consulta a la base de datos de forma oculta en un getter estático.
  2. Viola la separación de capas: los componentes de UI no deben actuar como proveedores estáticos de datos para otras pantallas.

### 1.2 Sesión Global Estática (`SessionManager`)
* **Archivo:** `App/Servicios/SessionManager.cs` (Líneas 5-13)
* **Código:**
  ```csharp
  public static class SessionManager
  {
      public static Usuario UsuarioActual { get; set; }
      public static bool IsLoggedIn => UsuarioActual != null;
  }
  ```
* **Problema:** Almacenamiento de sesión en una variable estática global en lugar de usar un servicio scoped o singleton inyectado (o un `AuthenticationStateProvider`).
* **Impacto:** Impide el desacoplamiento y no es seguro si la aplicación evoluciona o se prueba en paralelo.

### 1.3 Servicios de Notificación y Configuración como Clases Estáticas (`BusinessConfig`, `ToastService`, `CustomMessageBox`)
* **Archivos:**
  * `App/Servicios/BusinessConfig.cs` (Línea 18)
  * `App/Servicios/ToastService.cs` (Línea 5)
  * `App/Servicios/CustomMessageBox.cs` (Línea 5)
* **Problema:** En lugar de registrarse en `App.xaml.cs` bajo el contenedor de DI (`IServiceCollection`), estos servicios se definen 100% estáticos con delegados y eventos estáticos (`public static event Action...`).
* **Impacto:** Puede generar fugas de memoria si los componentes que se suscriben a eventos estáticos no se eliminan adecuadamente, además de acoplar todo el código al estado estático.

### 1.4 Instanciación Manual de Repositorios Ignorando la Inyección de Dependencias
* **Archivos afectados:** Prácticamente todos los componentes en `App/Pages/` (`Cobro.razor`, `Pedidos.razor`, `Clientes.razor`, `Empleados.razor`, `CambiarEstado.razor`, etc.).
* **Problema:** Aunque en `App.xaml.cs` (Líneas 24-34) se registran explícitamente como servicios singleton (`serviceCollection.AddSingleton<PedidoRepositorio>()`, etc.), en todas las páginas se instancian manualmente:
  ```csharp
  private PedidoRepositorio _pedidoRepo = new();
  private ClienteRepositorio _clienteRepo = new();
  ```
* **Impacto:** Duplicación inútil de instancias, pérdida del control de ciclo de vida del contenedor DI y falta de consistencia arquitectónica.

---

## 2. VALORES HARDCODEADOS Y DATOS FICTICIOS EN NEGOCIO / UI

### 2.1 Asignación Heurística Hardcodeada de Máquinas (Falsa Asignación)
* **Archivos:**
  * `App/Pages/Dashboard.razor` (Línea 388)
  * `App/Pages/Cobro.razor` (Línea 286)
  * `App/Pages/Pedidos.razor` (Línea 369)
  * `App/Pages/Finanzas.razor` (Línea 479)
* **Código:**
  ```csharp
  string machineName = !string.IsNullOrEmpty(p.MaquinaAsignada) ? p.MaquinaAsignada : "Sin Asignar";
  if (string.IsNullOrEmpty(p.MaquinaAsignada) && maquinasDb.Count > 0)
  {
      var assignedMachine = maquinasDb[(p.IdPedido - 1) % maquinasDb.Count];
      machineName = assignedMachine.Nombre;
  }
  ```
* **Problema:** Si un pedido no tiene máquina asignada en la base de datos (`MaquinaAsignada` es vacío), el sistema **inventa** que el pedido fue atendido por una máquina utilizando la fórmula matemática `(IdPedido - 1) % maquinasDb.Count`.
* **Impacto:** Presenta información falsa al usuario y distorsiona el seguimiento operativo. Si una máquina no fue asignada, debe mostrarse claramente `"Sin Asignar"`.

### 2.2 Servicio ID = 1 Hardcodeado en la Creación de Pedidos
* **Archivo:** `App/Pages/EditarPedido.razor` (Línea 483)
* **Código:**
  ```csharp
  var detalles = new List<DetallePedido>
  {
      new DetallePedido { IdServicio = 1, Cantidad = Weight, PrecioUnitario = (decimal)PricePerKg, Subtotal = (decimal)TotalCost }
  };
  ```
* **Problema:** Al crear un pedido desde esta pantalla, siempre se inserta un detalle con `IdServicio = 1` de forma fija, asumiendo que todos los pedidos son de "Lavado General" (o el servicio con ID 1).
* **Impacto:** Si se desea registrar un pedido de solo secado o planchado, o si los identificadores de la base de datos cambian, la orden se registra con el servicio incorrecto.

### 2.3 Peso (Kg) Asumido o Inventado por Defecto
* **Archivos:**
  * `App/Pages/Cobro.razor` (Línea 294)
  * `App/Pages/Pedidos.razor` (Línea 375)
  * `App/Pages/Finanzas.razor` (Línea 474)
* **Código:**
  ```csharp
  double peso = lavadoDetalle != null ? lavadoDetalle.Cantidad : (double)(p.Total / activePrice);
  ```
* **Problema:** Si un pedido no contiene el servicio con ID 1, el sistema calcula un "peso" dividiendo el monto `Total / activePrice`, o asigna `10.0 kg` fijos (`Finanzas.razor` L364).
* **Impacto:** Un pedido de $300 en servicio de secado o planchado aparecerá reportado con un peso ficticio en kilogramos.

### 2.4 Fondo de Caja Fijo en Finanzas
* **Archivo:** `App/Pages/Finanzas.razor` (Línea 424)
* **Código:**
  ```csharp
  EfectivoEnCajon = 500.00m + pagosEfectivoHoy - GastosHoyTotal;
  ```
* **Problema:** Se encuentra hardcodeado un fondo inicial de caja de **$500.00 MXN**.
* **Impacto:** Si en la realidad se abre caja con otro monto, el cálculo del efectivo en cajón será permanentemente erróneo.

### 2.5 Textos y Métricas Fijas en Reportes y Tickets
* **Archivos:**
  * `App/Modelos/Ticket.cs` (Líneas 24-27): Encabezado (`"LAVANDERÍA VILLAS DEL SUR"`), Dirección y Teléfono están hardcodeados como valores por defecto del modelo en vez de poblarse desde `BusinessConfig.Current`.
  * `App/Pages/Reportes.razor` (Líneas 52, 112, 117, 122):
    * Crecimiento mensual KPI: `▲ +18.4% vs mes anterior` (estático).
    * Hora Pico: `10:00 AM — 01:00 PM (45% de clientes)` (estático).
    * Servicio Más Rentable: `Lavado por Kg (68% de ingresos)` (estático).
    * Mayor Insumo Consumido: `Detergente Industrial (12.5L / semana)` (estático).
    * Curva SVG del gráfico (Líneas 99-100): Puntos SVG estáticos (`M 0 140 Q 100 110...`), no calculados desde la base de datos.

### 2.6 Datos Ficticios de Empleados
* **Archivos:** `App/Pages/Empleados.razor` (Línea 288-292) y `App/Pages/EditarEmpleado.razor` (Línea 261-270)
* **Problema:** La entidad real `Usuario` en base de datos no tiene columnas de salario, edad, turno ni teléfono, por lo que la UI rellena con datos ficticios (`PagoMes = 0.00m`, `Edad = 19`, `Password = "password123"`, `UltimoAcceso = "Hace 10 horas"`).

---

## 3. RUTAS ABSOLUTAS HARDCODEADAS DEPENDIENTES DE LA MÁQUINA LOCAL

* **Archivos:**
  * `App/Pages/Dashboard.razor` (Línea 466)
  * `App/Pages/Finanzas.razor` (Línea 547)
* **Código:**
  ```csharp
  string scriptPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "generar_pdf_actividad.py");
  if (!File.Exists(scriptPath))
  {
      scriptPath = @"C:\Users\Yadie\RiderProjects\LavanderiaApp0.1\App\generar_pdf_actividad.py";
  }
  ```
* **Problema:** Existe un respaldo (`fallback`) hardcodeado a la ruta absoluta del usuario original (`C:\Users\Yadie\...`).
* **Impacto:** Si el proyecto se ejecuta en otra computadora, otro directorio o en producción, cuando el archivo no esté en `BaseDirectory`, intentará buscar en `C:\Users\Yadie\...` y fallará lanzando una excepción.

---

## 4. FALLOS Y CONTRADICCIONES LÓGICAS CRÍTICAS ENTRE MÓDULOS

### 4.1 Contradicción Crítica en los Estados del Pedido (Incompatibilidad Total)
Existe una **desconexión completa** entre los estados definidos en el modelo, los que guarda la pantalla de cambio de estado y los que filtran o cuentan el Dashboard y las listas:

| Componente | Estados que utiliza o evalúa |
| :--- | :--- |
| **`Pedido.cs` (Modelo L14, L73-77)** | `"En espera"`, `"En proceso"`, `"Listo"`, `"Entregado"`, `"Cancelado"` |
| **`CambiarEstado.razor` (Líneas 36-75)** | `"En Lavado"`, `"En espera de lavado"`, `"En Secado"`, `"En espera de secado"`, `"Listo para entregar"`, `"En espera"` |
| **`Dashboard.razor` (Línea 347)** | Busca `p.Estado.Equals("Lavando")` o `p.Estado.Equals("Secando")` |
| **`Pedidos.razor` (Línea 316)** | Filtros: `"EN ESPERA DE ENTREGA"`, `"SECANDO"`, `"LAVANDO"`, `"ENTREGADO"` |

* **Consecuencia del fallo:**
  1. Si un usuario entra a `CambiarEstado.razor` y cambia el pedido a **`"En Lavado"`**:
     * En el **Dashboard**, `EnCursoCount` dará `0` porque compara contra `"Lavando"` (sin el prefijo "En ").
     * En **Pedidos.razor**, al filtrar por la pestaña `"LAVANDO"`, el pedido desaparecerá porque compara igualdad exacta (`o.Estado.Equals("LAVANDO")`).
     * Las propiedades helper de `Pedido.cs` (`EsEnProceso`, `EsListo`) retornarán `false`.

### 4.2 Contradicción Lógica en el Filtro de Puestos de Empleados
* **Archivo:** `App/Pages/Empleados.razor` (Líneas 253 y 283)
* **Código:**
  ```csharp
  // Opciones del filtro (Línea 253)
  private readonly string[] PosOptions = new[] { "TODOS", "Recepcionista", "Lavadero", "Supervisor" };

  // Asignación en carga de empleados (Línea 283)
  Posicion = u.Rol == "Admin" ? "Administrador" : "Operador",
  ```
* **Problema:** Los empleados siempre tienen en la propiedad `Posicion` el valor `"Administrador"` o `"Operador"`. Sin embargo, el filtro de la interfaz busca `"Recepcionista"`, `"Lavadero"` o `"Supervisor"`.
* **Impacto:** Si el usuario selecciona cualquier filtro distinto de `"TODOS"`, la lista de empleados quedará vacía siempre.

---

## 5. ERRORES DE CONCURRENCIA, GESTIÓN DE TRANSACCIONES Y EXCEPCIONES SILENCIADAS

### 5.1 Consultas Fuera de la Transacción y Condición de Carrera en Inventario
* **Archivo:** `App/Servicios/InventarioAutomatizacion.cs` (Líneas 19-53)
* **Código:**
  ```csharp
  // LÍNEA 23: Se consulta fuera de transacción
  string queryCheck = "SELECT InventarioRestado FROM Pedidos WHERE IdPedido = @IdPedido";
  ...
  // LÍNEA 37: Se consultan detalles fuera de transacción
  string queryDetalles = "SELECT IdServicio, Cantidad FROM DetallesPedido WHERE IdPedido = @IdPedido";
  ...
  // LÍNEA 52: Recién aquí se inicia la transacción
  using var transaccion = conexion.BeginTransaction();
  ```
* **Problema:** La verificación de si el pedido ya fue procesado (`InventarioRestado == 1`) y la lectura de los detalles del pedido se realizan **antes** de abrir la transacción (`BeginTransaction()`).
* **Impacto:** Si dos solicitudes procesan el mismo pedido casi simultáneamente, ambas leerán `InventarioRestado == 0` y ejecutarán la deducción de inventario por duplicado.

### 5.2 Excepciones Silenciadas al Restar Inventario
* **Archivo:** `App/Servicios/InventarioAutomatizacion.cs` (Línea 138)
* **Código:**
  ```csharp
  catch (Exception ex)
  {
      System.Diagnostics.Debug.WriteLine($"Error general en automatización de inventario: {ex.Message}");
  }
  ```
* **Problema:** Si el método falla (por ejemplo, base de datos bloqueada o insumo inexistente), la excepción es capturada y silenciada.
* **Impacto:** Las pantallas que llaman a `ProcesarConsumoInventario` (`CambiarEstado.razor` y `EditarPedido.razor`) asumirán que el inventario se descontó correctamente sin informar del error al usuario.

### 5.3 Parseo Inseguro de Fechas (`DateTime.Parse`) en Repositorios
* **Archivo:** `App/Repositorios/PedidoRepositorio.cs` (Líneas 48, 49, 78, 79)
* **Código:**
  ```csharp
  FechaRecepcion = System.DateTime.Parse(reader.GetString(reader.GetOrdinal("FechaRecepcion"))),
  ```
* **Problema:** Se utiliza `DateTime.Parse` sin indicar `CultureInfo.InvariantCulture`.
* **Impacto:** Las fechas guardadas en SQLite como `"yyyy-MM-dd HH:mm:ss"` pueden fallar con `FormatException` en equipos con configuraciones regionales donde el orden de día y mes difiera.

---

## 6. MATRIZ DE RESUMEN Y PRIORIDAD DE CORRECCIÓN

| Prioridad | Categoría | Archivo(s) Principal(es) | Hallazgo | Acción Recomendada |
| :---: | :---: | :---: | :---: | :---: |
| **CRÍTICA** | **Lógica / Contradicción** | `CambiarEstado.razor`<br>`Dashboard.razor`<br>`Pedidos.razor`<br>`Pedido.cs` | **Incompatibilidad de estados:** Los nombres de estado asignados no coinciden con los que filtran y cuentan las pantallas. | Unificar los nombres de estado en una constante o `enum` global compartido (`EstadoPedido`). |
| **CRÍTICA** | **Lógica / Datos** | `Dashboard.razor`<br>`Cobro.razor`<br>`Pedidos.razor`<br>`Finanzas.razor` | **Asignación heurística falsa de máquinas:** `(IdPedido - 1) % maquinasDb.Count` inventa máquinas cuando no están asignadas. | Eliminar la heurística de residuo `%` y mostrar `"Sin Asignar"` cuando `MaquinaAsignada` esté vacío. |
| **ALTA** | **Concurrencia / Datos** | `InventarioAutomatizacion.cs` | **Condición de carrera y silenciado de errores:** Lecturas previas a transacción y captura vacía de excepciones. | Mover comprobaciones dentro de `BeginTransaction()` y relanzar o propagar errores al usuario. |
| **ALTA** | **Arquitectura / Estático** | `Maquinas.razor` (L256)<br>`MainLayout.razor` (L641)<br>`Dashboard.razor` (L331) | **Propiedad estática UI `SharedMachines`:** Acoplamiento directo a componente Razor instanciando repositorios en el getter. | Reemplazar por un servicio inyectado (`IMaquinaServicio` o `MaquinaRepositorio`). |
| **MEDIA** | **Portabilidad / Hardcodeo** | `Dashboard.razor` (L466)<br>`Finanzas.razor` (L547) | **Ruta absoluta hardcodeada al usuario local (`C:\Users\Yadie\...`)** al llamar al script de PDF. | Usar únicamente rutas relativas a `AppDomain.CurrentDomain.BaseDirectory`. |
| **MEDIA** | **Lógica / Contradicción** | `Empleados.razor` (L253 vs L283) | **Filtros incompatibles:** Filtro por `"Recepcionista"`, `"Lavadero"` no coincide con `Posicion` (`"Operador"`). | Ajustar opciones del filtro a los puestos/roles reales de la base de datos. |
| **MEDIA** | **Hardcodeo** | `Finanzas.razor` (L424)<br>`EditarPedido.razor` (L483) | **Fondo de caja fijo ($500 MXN)** e **IdServicio = 1 fijo** al registrar pedidos. | Parametrizar fondo de caja e id de servicio seleccionado por el usuario. |
| **BAJA** | **Hardcodeo UI** | `Reportes.razor`<br>`Ticket.cs` | **Textos estáticos en gráficos y KPIs** en pantalla de Reportes y datos fijos de membrete en `Ticket`. | Conectar KPIs y SVG a métricas dinámicas de `ReporteActual` y `BusinessConfig`. |

---

## 7. ESTADO DE RESOLUCIÓN Y CORRECCIÓN (APLICADO EN CÓDIGO)

Con fecha **13 de Julio de 2026**, se aplicaron de forma exitosa las correcciones en el código fuente para solucionar los problemas reportados:

1. **Unificación de Estados del Pedido y Propiedades Rápidas (`Pedido.cs`, `Dashboard.razor`, `Pedidos.razor`):**
   - Se actualizaron las propiedades helpers de `Pedido` (`EsEnEspera`, `EsEnLavado`, `EsEnSecado`, `EsEnProceso`, `EsListo`, `EsEntregado`, `EsCancelado`) para reconocer tanto los estados con prefijo (`"En Lavado"`, `"En Secado"`) como sin prefijo (`"Lavando"`, `"Secando"`).
   - Se actualizó `Dashboard.razor` para que `EnCursoCount` y las etiquetas visuales utilicen `EsEnProceso` y soporten la gama completa de estados.
   - Se actualizó `Pedidos.razor` con un comparador flexible (`CoincideEstado`) y opciones de filtro alineadas al dominio del negocio.

2. **Eliminación de la Asignación Heurística Ficticia de Máquina y Peso (`Dashboard.razor`, `Cobro.razor`, `Pedidos.razor`, `Finanzas.razor`):**
   - Se eliminó la fórmula `(IdPedido - 1) % maquinasDb.Count` en todas las vistas. Si un pedido no tiene máquina guardada, se muestra de forma transparente `"Sin Asignar"`.
   - Se corrigió el cálculo de peso para sumar la cantidad real de los servicios contratados en lugar de asumir 10 kg fijos.

3. **Eliminación de Rutas Absolutas Hardcodeadas (`Dashboard.razor`, `Finanzas.razor`):**
   - Se reemplazó el fallback absoluto (`C:\Users\Yadie\...`) por resolución relativa a `AppDomain.CurrentDomain.BaseDirectory`.

4. **Corrección de Concurrencia y Transacciones en Deducción de Inventario (`InventarioAutomatizacion.cs`):**
   - Se movió la comprobación anti-duplicidad (`InventarioRestado == 1`) y todas las consultas de inventario dentro del bloque transaccional (`BeginTransaction()`).
   - Se eliminó el silenciamiento de excepciones de base de datos (`throw;`) para garantizar la integridad de datos.

5. **Robustez en Parseo de Fechas (`PedidoRepositorio.cs`):**
   - Se añadió `System.Globalization.CultureInfo.InvariantCulture` en `DateTime.Parse` al leer fechas desde SQLite.

6. **Alineación de Filtros de Roles de Empleados (`Empleados.razor`):**
   - Se actualizaron las opciones del filtro (`"Administrador"`, `"Operador"`, `"Recepcionista"`, `"Supervisor"`) y el comparador para evaluar tanto `Posicion` como `Rol`.

7. **Configuración Dinámica en Comprobantes (`Ticket.cs`):**
   - Se conectaron el encabezado, dirección, teléfono y pie de página del ticket con `BusinessConfig.Current`.

---
*Fin del Reporte y Registro de Corrección.*
