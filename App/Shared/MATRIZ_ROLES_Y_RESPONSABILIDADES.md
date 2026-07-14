# División de Roles y Responsabilidades (Administrador y Empleado)

En **Lavandería Pro (v0.1)** hemos simplificado el sistema para operar con **2 roles claros**:

1. **Administrador (`Admin`)**: Control total del negocio, finanzas, personal y configuraciones.
2. **Empleado (`Empleado`)**: Operación diaria en mostrador, taller e inventario.

---

## 1. Matriz de Acceso a Pestañas (Menú Lateral)

| Pestaña del Menú | 1. Administrador (`admin`) | 2. Empleado (`empleado`) | Propósito y Explicación |
| :--- | :---: | :---: | :--- |
| **General** (`/dashboard`) | 🟢 **Sí** | 🟢 **Sí** | Vista general del día y estadísticas operativas. |
| **Pedidos** (`/pedidos`) | 🟢 **Sí** | 🟢 **Sí** | Recepción de prendas, creación de pedidos y cambio de estado en máquinas. |
| **Clientes** (`/clientes`) | 🟢 **Sí** | 🟢 **Sí** | Alta y búsqueda de clientes en mostrador. |
| **Inventario** (`/inventario`) | 🟢 **Sí** | 🟢 **Sí** | Control y consumo de detergentes, suavizantes e insumos del taller. |
| **Caja Chica** (Gastos / Turno) | 🟢 **Sí** | 🟢 **Sí** | Registro de gastos diarios en mostrador y cierre de turno. |
| **Empleados** (`/empleados`) | 🟢 **Sí** | 🔴 **Oculto** | **Exclusivo de Administrador** (gestión de personal y altas/bajas). |
| **Finanzas** (`/finanzas`) | 🟢 **Sí** | 🔴 **Oculto** | **Exclusivo de Administrador** (reportes financieros ejecutivos e ingresos). |
| **Configuraciones** (`/configuraciones`) | 🟢 **Sí** | 🔴 **Oculto** | **Exclusivo de Administrador** (tarifas de servicios, tickets y base de datos). |

---

## 2. Usuarios y Credenciales en Base de Datos (`Lavanderia.db`)

Se han depurado los demás usuarios para dejar únicamente estas 2 cuentas en la base de datos:

| Rol Operativo | Nombre en el Sistema | Usuario | Contraseña | Acceso |
| :--- | :--- | :---: | :---: | :--- |
| **Administrador** | Administrador General | **`admin`** | `admin123` | **Total (8 pestañas)** |
| **Empleado** | Empleado General | **`empleado`** | `empleado123` | **Operativo (5 pestañas)** |
