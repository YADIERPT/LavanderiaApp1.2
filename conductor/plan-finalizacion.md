# Plan de Finalización del Proyecto LavanderiaApp

Este plan detalla los pasos necesarios para completar el desarrollo de la aplicación de lavandería, pasando de un prototipo básico a una aplicación funcional con autenticación, gestión de servicios detallados y pagos.

## 1. Refactorización de Modelos
Ajustar las clases del modelo para reflejar correctamente la estructura de la base de datos y las necesidades del negocio.

- **Cliente.cs**: Eliminar la herencia de `Usuario`.
- **DetallePedido.cs**: Añadir `IdPedido`, `IdServicio` y `PrecioUnitario`.
- **Pago.cs**: Añadir `IdPedido` para vincular pagos con pedidos.
- **Usuario.cs**: Asegurar que coincida con la tabla de la base de datos.

## 2. Actualización de la Base de Datos
Modificar `DatabaseInitializer.cs` para incluir las tablas faltantes y datos iniciales.

- **Tablas a crear**:
    - `Servicios`: `IdServicio`, `Nombre`, `Descripcion`, `Precio`, `TiempoEstimado`.
    - `DetallesPedido`: `IdDetallePedido`, `IdPedido`, `IdServicio`, `Cantidad`, `PrecioUnitario`, `Subtotal`.
    - `Pagos`: `IdPago`, `IdPedido`, `MetodoPago`, `FechaPago`, `MontoPago`.
- **Datos Semilla**: Insertar servicios básicos (Lavado, Secado, Planchado).

## 3. Implementación de Repositorios
Desarrollar la lógica de acceso a datos en la carpeta `Repositorios`.

- **UsuarioRepositorio**: Métodos para buscar por nombre de usuario (para el login).
- **ServicioRepositorio**: Listar servicios disponibles.
- **PagoRepositorio**: Registrar nuevos pagos.
- **DetallePedidoRepositorio**: Guardar los detalles de un pedido.

## 4. Lógica de Negocio y Sesión
Implementar los servicios y un gestor de sesión.

- **SessionManager.cs**: Clase estática para almacenar el `Usuario` actual que ha iniciado sesión.
- **LoginServicio**: Lógica para validar credenciales.
- **PagoServicio**: Procesar el pago y actualizar el estado del pedido.

## 5. Desarrollo de la Interfaz de Usuario (WPF)
Completar las vistas y su lógica.

### 5.1. Pantalla de Login (inicio.xaml)
- Crear diseño con campos para Usuario y Contraseña.
- Implementar lógica en `inicio.xaml.cs` para validar contra `LoginServicio` y abrir `MainWindow`.

### 5.2. Ventana Principal (Interfaz.xaml)
- Actualizar `App.xaml` para que la app inicie en la pantalla de login.
- En la pestaña de **Pedidos**:
    - Permitir seleccionar múltiples servicios.
    - Mostrar el usuario actual (obtenido de `SessionManager`).
    - Integrar la lógica de "Entregar y Pagar" con el registro real de un pago en la base de datos.

## 6. Verificación y Pruebas
- Realizar pruebas de flujo completo: Login -> Registro de Cliente -> Nuevo Pedido con Servicios -> Cambio de Estado -> Pago y Entrega.
- Verificar la persistencia en la base de datos SQLite.
