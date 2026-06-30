using Microsoft.Data.Sqlite;

namespace LavanderiaApp;

public class DatabaseInitializer
{
    public static void Inicializar()
    {
        using var conexion = new SqliteConnection(Config.ConnectionString);
        conexion.Open();

        // 1. Crear tablas base si no existen (sin las columnas nuevas que se migrarán luego)
        string baseSchema = @"
            PRAGMA foreign_keys = ON;

            CREATE TABLE IF NOT EXISTS Clientes (
                IdCliente INTEGER PRIMARY KEY AUTOINCREMENT,
                Nombre TEXT NOT NULL,
                Telefono TEXT NOT NULL,
                Direccion TEXT NOT NULL
            );
            
            CREATE TABLE IF NOT EXISTS Usuarios (
                IdUsuario INTEGER PRIMARY KEY AUTOINCREMENT,
                Nombre TEXT NOT NULL,
                NombreUsuario TEXT NOT NULL UNIQUE,
                Password TEXT NOT NULL,
                Rol TEXT NOT NULL
            );

            CREATE TABLE IF NOT EXISTS Pedidos (
                IdPedido INTEGER PRIMARY KEY AUTOINCREMENT,
                IdCliente INTEGER NOT NULL,
                IdUsuario INTEGER NOT NULL,
                FechaRecepcion TEXT NOT NULL,
                FechaEntrega TEXT,
                Total DECIMAL NOT NULL,
                FOREIGN KEY (IdCliente) REFERENCES Clientes(IdCliente),
                FOREIGN KEY (IdUsuario) REFERENCES Usuarios(IdUsuario)
            );

            CREATE TABLE IF NOT EXISTS Servicios (
                IdServicio INTEGER PRIMARY KEY AUTOINCREMENT,
                Nombre TEXT NOT NULL,
                Descripcion TEXT,
                Precio DECIMAL NOT NULL,
                TiempoEstimado INTEGER NOT NULL
            );

            CREATE TABLE IF NOT EXISTS DetallesPedido (
                IdDetallePedido INTEGER PRIMARY KEY AUTOINCREMENT,
                IdPedido INTEGER NOT NULL,
                IdServicio INTEGER NOT NULL,
                Cantidad REAL NOT NULL,
                PrecioUnitario DECIMAL NOT NULL,
                Subtotal DECIMAL NOT NULL,
                FOREIGN KEY (IdPedido) REFERENCES Pedidos(IdPedido),
                FOREIGN KEY (IdServicio) REFERENCES Servicios(IdServicio)
            );

            CREATE TABLE IF NOT EXISTS Pagos (
                IdPago INTEGER PRIMARY KEY AUTOINCREMENT,
                IdPedido INTEGER NOT NULL,
                MetodoPago TEXT NOT NULL,
                FechaPago TEXT NOT NULL,
                MontoPago DECIMAL NOT NULL,
                FOREIGN KEY (IdPedido) REFERENCES Pedidos(IdPedido)
            );
        ";

        using (var cmd = new SqliteCommand(baseSchema, conexion))
        {
            cmd.ExecuteNonQuery();
        }

        // 2. MIGRACIONES (Agregar columnas si faltan)
        try
        {
            using var cmdAlter = new SqliteCommand("ALTER TABLE Pedidos ADD COLUMN Estado TEXT NOT NULL DEFAULT 'En espera';", conexion);
            cmdAlter.ExecuteNonQuery();
        } catch { }

        try
        {
            using var cmdAlter = new SqliteCommand("ALTER TABLE Servicios ADD COLUMN UnidadMedida TEXT NOT NULL DEFAULT 'Unidad';", conexion);
            cmdAlter.ExecuteNonQuery();
        } catch { }

        // 3. INSERTAR DATOS SEMILLA (Ahora que las columnas existen)
        string seedData = @"
            INSERT OR IGNORE INTO Usuarios (IdUsuario, Nombre, NombreUsuario, Password, Rol)
            VALUES (1, 'Administrador', 'admin', 'admin123', 'Admin');

            INSERT OR IGNORE INTO Servicios (IdServicio, Nombre, Descripcion, Precio, TiempoEstimado, UnidadMedida) VALUES 
            (1, 'Lavado General', 'Lavado de ropa por kilogramo', 15.00, 120, 'Kg'),
            (2, 'Secado', 'Secado de ropa por carga', 30.00, 60, 'Carga'),
            (3, 'Planchado', 'Planchado por prenda (Opcional)', 10.00, 15, 'Unidad');
            
            -- Asegurar que los servicios existentes tengan las unidades correctas
            UPDATE Servicios SET UnidadMedida = 'Kg' WHERE IdServicio = 1;
            UPDATE Servicios SET UnidadMedida = 'Carga' WHERE IdServicio = 2;
            UPDATE Servicios SET UnidadMedida = 'Unidad' WHERE IdServicio = 3;
        ";

        using (var cmd = new SqliteCommand(seedData, conexion))
        {
            cmd.ExecuteNonQuery();
        }
    }
}
