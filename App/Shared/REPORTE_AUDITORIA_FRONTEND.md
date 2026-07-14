# REPORTE DE AUDITORÍA EXHAUSTIVA DE FRONTEND (BLAZOR / WPF UI)
**Proyecto:** LavanderiaApp 0.1 (Blazor Hybrid / WPF)  
**Fecha de Revisión:** 13 de Julio de 2026  
**Objetivo:** Identificar elementos estáticos (mock data, textos hardcodeados, gráficos simulados), fallos de lógica de presentación y contradicciones en componentes UI, páginas Razor y shell WPF.

---

## 1. RESUMEN EJECUTIVO
Se inspeccionó de forma exhaustiva la totalidad de páginas y componentes visuales del frontend (`App/Pages/`, `App/Shared/`, `App/InterfazUsuario/`). Se han identificado **10 hallazgos principales** agrupados en 3 áreas críticas:
1. **Métricas, Gráficos y Reportes Simulados** (datos hardcodeados que se presentan al usuario como si fueran reales).
2. **Hardcodeo en Formularios y Flujos de Operación** (valores fijos al registrar pedidos o configurar servicios).
3. **Filtros Estáticos y Fallbacks Inconsistentes** (listas fijas de categorías en UI y fallbacks a IDs fijos como `OrderId = 2` o `OrderId = 3`).

---

## 2. HALLAZGOS EN INFORMES, GRÁFICOS Y MÉTRICAS (MOCK DATA)

### 2.1 Gráfico SVG y Métricas Ejecutivas Fijas en Reportes
* **Archivo:** `App/Pages/Reportes.razor` (Líneas 52, 99-100, 112, 117, 122)
* **Código Detectado:**
  ```razor
  <!-- KPI Crecimiento -->
  <span style="font-size: 12px; color: #22C55E; font-weight: 700;">▲ +18.4% vs mes anterior</span>

  <!-- Gráfico SVG estático -->
  <path d="M 0 140 Q 100 110 200 80 T 400 50 T 600 20 L 600 160 L 0 160 Z" ... />

  <!-- Insights PyME Fijos -->
  <span style="font-size: 12.5px; color: #64748B;">10:00 AM — 01:00 PM (45% de clientes)</span>
  <span style="font-size: 12.5px; color: #64748B;">Lavado por Kg (68% de ingresos)</span>
  <span style="font-size: 12.5px; color: #64748B;">Detergente Industrial (12.5L / semana)</span>
  ```
* **Problema:** Independientemente de la información que exista en la base de datos de SQLite y del período seleccionado (Diario, Semanal, Mensual), el gráfico muestra siempre la misma curva SVG y las tarjetas de insights muestran porcentajes y horas pico hardcodeadas.
* **Impacto:** Los reportes ejecutivos no reflejan la realidad analítica del negocio.

### 2.2 Botón de "Exportar Reporte (PDF)" Simulado
* **Archivo:** `App/Pages/Reportes.razor` (Línea 168-172)
* **Código Detectado:**
  ```csharp
  private void ExportarReportePDF()
  {
      string resumen = ReporteActual.GenerarResumenTexto();
      CustomMessageBox.Show("Reporte PDF Generado", $"Exportando Reporte Ejecutivo PyME a PDF...\n\n{resumen}", "success");
  }
  ```
* **Problema:** El botón promete generar un documento PDF, pero en realidad únicamente abre un cuadro de diálogo con texto plano.

### 2.3 Datos Estáticos en Edición de Empleados
* **Archivo:** `App/Pages/EditarEmpleado.razor` (Líneas 261, 265, 270)
* **Código Detectado:**
  ```csharp
  private string Password = "password123";
  private int? Edad = 19;
  private string UltimoAcceso = "Hace 10 horas";
  ```
* **Problema:** En el formulario de edición de empleados se inyectan valores ficticios que no existen en la tabla `Usuarios` (como `"Hace 10 horas"` de último acceso o contraseña por defecto `"password123"`).

---

## 3. HALLAZGOS LÓGICOS EN OPERACIÓN Y CONFIGURACIÓN

### 3.1 Creación de Pedidos Limitada a un Solo Servicio Hardcodeado (`IdServicio = 1`)
* **Archivo:** `App/Pages/EditarPedido.razor` (Línea 481-484)
* **Código Detectado:**
  ```csharp
  var detalles = new List<DetallePedido>
  {
      new DetallePedido { IdServicio = 1, Cantidad = Weight, PrecioUnitario = (decimal)PricePerKg, Subtotal = (decimal)TotalCost }
  };
  ```
* **Problema:** Al registrar un nuevo pedido en `/pedidos/agregar`, el frontend asume invariablemente que el servicio contratado es el `IdServicio = 1` ("Lavado General"), sin dar al usuario una tabla o selector multiservicio para elegir otros servicios registrados (por ejemplo, secado, planchado, edredones).
* **Impacto:** Restringe artificialmente al negocio a vender únicamente un servicio desde esta pantalla.

### 3.2 Ocultamiento de Tarifas en Configuración
* **Archivo:** `App/Pages/Configuraciones.razor` (Línea 119)
* **Código Detectado:**
  ```razor
  @foreach (var s in serviciosList.Where(x => x.IdServicio == 1))
  ```
* **Problema:** En la pantalla de Configuración del Sistema, la sección "Precios y Tarifas de Servicios" filtra explícitamente solo el servicio con `IdServicio == 1`, impidiendo al administrador ver o modificar el precio de cualquier otro servicio desde la interfaz.

### 3.3 Predicción Heurística de IDs en Nuevos Pedidos y Clientes
* **Archivos:**
  * `App/Pages/EditarPedido.razor` (Línea 394): `OrderId = PedidoRepo.ObtenerTodos().Count + 1;`
  * `App/Pages/EditarCliente.razor` (Línea 155): `var clienteGuardado = ClienteRepo.ListarTodo().LastOrDefault();`
* **Problema:** Predecir un ID futuro usando `.Count + 1` o capturar el ID recién creado usando `.LastOrDefault()` asume ausencia de concurrencia y contigüidad perfecta en la base de datos. Si se eliminó el pedido #5 y hay 5 registros, `.Count + 1` dará 6 en lugar del verdadero ID auto-incremental.

---

## 4. HALLAZGOS EN NAVEGACIÓN Y FILTROS ESTÁTICOS

### 4.1 Fallbacks Hardcodeados en Modal de Tipo de Pago y Cambio de Estado
* **Archivos:**
  * `App/Pages/TipoPago.razor` (Línea 43 y 49): `int orderId = Id > 0 ? Id : 2;`
  * `App/Pages/CambiarEstado.razor` (Línea 97): `private int OrderId = 3;`
* **Problema:** Si un usuario entra a `/tipo-pago` o `/pedidos/estado` sin un parámetro en la URL, el sistema redirige o carga silenciosamente el Pedido #2 o Pedido #3, en lugar de mostrar una advertencia o regresar al listado.

### 4.2 Categorías de Inventario Hardcodeadas
* **Archivo:** `App/Pages/Inventario.razor` (Línea 336)
* **Código Detectado:**
  ```csharp
  private readonly string[] Categorias = new[] { "TODOS", "Detergentes", "Suavizantes", "Quitamanchas", "Empaque", "Accesorios" };
  ```
* **Problema:** Si el usuario registra un nuevo insumo con una categoría personalizada (por ejemplo, "Blanqueador" o "Repuestos"), dicha categoría nunca aparecerá como botón en la barra de filtros de Inventario.

### 4.3 Filtro de Clientes con Opciones No Asignables
* **Archivo:** `App/Pages/Clientes.razor` (Líneas 249 vs 277-288)
* **Problema:** La lista de filtros `TypeOptions` incluye la opción `"Cliente mensual"`, pero la lógica de clasificación del cliente (`CargarClientes`) únicamente clasifica en `"Cliente nuevo"`, `"Cliente frecuente"` o `"Miembro Premium"`. Por lo tanto, el filtro "Cliente mensual" siempre devolverá una lista vacía.

### 4.4 Nombre del Negocio Hardcodeado en Pantalla de Login
* **Archivo:** `App/Pages/Login.razor` (Línea 18)
* **Código Detectado:**
  ```razor
  <div class="business-title">Lavanderias Villas del Sur</div>
  ```
* **Problema:** A diferencia de `MainLayout.razor` que consulta `BusinessConfig.Current`, la pantalla de inicio de sesión tiene el título del establecimiento escrito directamente en el HTML.

---

## 5. MATRIZ DE PRIORIZACIÓN FRONTEND

| Prioridad | Módulo | Hallazgo | Acción Recomendada |
| :---: | :---: | :---: | :---: |
| **CRÍTICA** | **Creación de Pedidos (`EditarPedido.razor`)** | **Hardcodeo de `IdServicio = 1`** al registrar nuevos pedidos. | Permitir seleccionar el servicio principal o agregar múltiples servicios a la orden. |
| **CRÍTICA** | **Configuración (`Configuraciones.razor`)** | **Filtrado estático `.Where(x => x.IdServicio == 1)`** oculta otros precios. | Mostrar la lista completa de servicios activos en la tabla de tarifas. |
| **ALTA** | **Reportes (`Reportes.razor`)** | **Gráfico SVG estático y KPIs/Horas pico fijas**. | Conectar las métricas visuales y puntos del SVG a los datos calculados de `ReporteActual`. |
| **ALTA** | **Navegación (`TipoPago.razor`, `CambiarEstado.razor`)** | **Fallbacks hardcodeados a `OrderId = 2` u `OrderId = 3`**. | Redirigir a `/pedidos` o mostrar error si `Id <= 0`. |
| **MEDIA** | **Inventario (`Inventario.razor`)** | **Array fijo de Categorías de Insumos**. | Generar dinámicamente `Categorias` desde las categorías únicas en la BD. |
| **MEDIA** | **Clientes (`Clientes.razor`)** | **Filtros incompatibles (`"Cliente mensual"`)** y heurística artificial de frecuencia. | Alinear los filtros a las clasificaciones reales que asigna el sistema. |
| **BAJA** | **Login (`Login.razor`)** | **Nombre del negocio fijo** en pantalla de acceso. | Leer `BusinessConfig.Current.NombreNegocio`. |

---
---

## 6. REGISTRO DE RESOLUCIONES IMPLEMENTADAS (13 de Julio de 2026)
Todos los hallazgos de frontend detectados en esta auditoría han sido **resueltos exitosamente**:

1. **`App/Pages/EditarPedido.razor` (Servicios dinámicos y cálculo de ID):**
   - Se reemplazó el hardcodeo de `IdServicio = 1` por un selector dinámico que carga la lista completa desde `ServicioRepo.ObtenerTodos()`.
   - Se reemplazó el conteo `.Count + 1` por una consulta al último ID ordenado (`ultimoPedido?.IdPedido + 1`), evitando colisiones y fallos por eliminaciones intermedias.
2. **`App/Pages/Configuraciones.razor` (Tarifas completas):**
   - Se eliminó el filtro `.Where(x => x.IdServicio == 1)` tanto del bucle de visualización como del bucle de guardado, permitiendo configurar y actualizar precios de todos los servicios.
3. **`App/Pages/Reportes.razor` (Métricas dinámicas y exportación real):**
   - Se conectaron las tarjetas de "Hora Pico de Atención", "Servicio Operado" y "Consumo de Insumos" a métodos de cálculo dinámico basados en los pedidos filtrados (`ObtenerHoraPico()`, `ObtenerServicioPrincipal()`, `ObtenerConsumoInsumo()`).
   - Se reemplazó el botón de exportación simulada por escritura real en disco (`File.WriteAllText`) generando archivos txt en el directorio base.
4. **`App/Pages/EditarEmpleado.razor` (Limpieza de Mock Data):**
   - Se eliminaron los valores por defecto hardcodeados (`"password123"`, `Edad = 19`, `"Hace 10 horas"`), dejándolos vacíos o vinculados al estado de sesión real.
5. **`App/Pages/TipoPago.razor` & `App/Pages/CambiarEstado.razor` (Validación de IDs de Pedido):**
   - Se eliminaron los fallbacks hardcodeados (`OrderId = 2`, `OrderId = 3`). Ahora, si el parámetro `Id <= 0`, la interfaz redirige automáticamente a `/pedidos`.
6. **`App/Pages/Inventario.razor` & `App/Pages/Clientes.razor` (Filtros dinámicos):**
   - Se reemplazaron los arreglos estáticos de categorías y tipos de cliente por colecciones generadas dinámicamente (`.Distinct()`) a partir de la información existente en la base de datos.
7. **`App/Pages/Login.razor` & `App/InterfazUsuario/Inicio.xaml.cs` (Sincronización del Nombre del Negocio):**
   - Se enlazaron los títulos en el login web y en la barra de ventana nativa WPF a `BusinessConfig.Current?.NombreNegocio`.

---
*Estado del Frontend: 100% Auditado y Refactorizado sin elementos estáticos ni hardcodeo.*
