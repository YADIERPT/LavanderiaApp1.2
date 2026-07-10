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

            CREATE TABLE IF NOT EXISTS Inventario (
                IdInsumo INTEGER PRIMARY KEY AUTOINCREMENT,
                Nombre TEXT NOT NULL,
                Categoria TEXT NOT NULL,
                Cantidad REAL NOT NULL,
                UnidadMedida TEXT NOT NULL,
                StockMinimo REAL NOT NULL,
                PrecioUnitario DECIMAL NOT NULL,
                UltimaActualizacion TEXT NOT NULL
            );

            CREATE TABLE IF NOT EXISTS Recetas (
                IdReceta INTEGER PRIMARY KEY AUTOINCREMENT,
                IdServicio INTEGER NOT NULL,
                IdInsumo INTEGER NOT NULL,
                Cantidad REAL NOT NULL,
                FOREIGN KEY (IdServicio) REFERENCES Servicios(IdServicio),
                FOREIGN KEY (IdInsumo) REFERENCES Inventario(IdInsumo)
            );

            CREATE TABLE IF NOT EXISTS Gastos (
                IdGasto INTEGER PRIMARY KEY AUTOINCREMENT,
                Monto DECIMAL NOT NULL,
                Concepto TEXT NOT NULL,
                Fecha TEXT NOT NULL
            );

            CREATE TABLE IF NOT EXISTS CortesCaja (
                IdCorte INTEGER PRIMARY KEY AUTOINCREMENT,
                Fecha TEXT NOT NULL,
                EfectivoReportado DECIMAL NOT NULL,
                EfectivoEsperado DECIMAL NOT NULL,
                Diferencia DECIMAL NOT NULL,
                Empleado TEXT NOT NULL
            );

            CREATE TABLE IF NOT EXISTS Auditorias (
                Id INTEGER PRIMARY KEY AUTOINCREMENT,
                Fecha TEXT NOT NULL,
                Usuario TEXT NOT NULL,
                Modulo TEXT NOT NULL,
                Accion TEXT NOT NULL,
                Detalle TEXT NOT NULL
            );

            CREATE TABLE IF NOT EXISTS Maquinas (
                IdMaquina INTEGER PRIMARY KEY,
                Nombre TEXT NOT NULL,
                Status TEXT NOT NULL,
                CiclosOperados INTEGER NOT NULL,
                ProxMantenimientoCiclos INTEGER NOT NULL,
                Observacion TEXT
            );

            CREATE TABLE IF NOT EXISTS AppMetadata (
                Clave TEXT PRIMARY KEY,
                Valor TEXT NOT NULL
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

        try
        {
            using var cmdAlter = new SqliteCommand("ALTER TABLE Pedidos ADD COLUMN InventarioRestado INTEGER NOT NULL DEFAULT 0;", conexion);
            cmdAlter.ExecuteNonQuery();
        } catch { }

        try
        {
            using var cmdAlter = new SqliteCommand("ALTER TABLE Pedidos ADD COLUMN CostoInsumos DECIMAL NOT NULL DEFAULT 0.0;", conexion);
            cmdAlter.ExecuteNonQuery();
        } catch { }

        try
        {
            using var cmdAlter = new SqliteCommand("ALTER TABLE Pedidos ADD COLUMN MaquinaAsignada TEXT;", conexion);
            cmdAlter.ExecuteNonQuery();
        } catch { }

        // 3. VERIFICAR Y REGISTRAR SEMILLA INICIALIZADA (Para no reinsertar máquinas borradas al reiniciar)
        bool semillaInicializada = false;
        try
        {
            using var cmdCheck = new SqliteCommand("SELECT COUNT(*) FROM AppMetadata WHERE Clave = 'SemillaInicializada';", conexion);
            semillaInicializada = Convert.ToInt32(cmdCheck.ExecuteScalar()) > 0;
            if (!semillaInicializada)
            {
                using var cmdCheckUser = new SqliteCommand("SELECT COUNT(*) FROM Usuarios;", conexion);
                if (Convert.ToInt32(cmdCheckUser.ExecuteScalar()) > 0)
                {
                    semillaInicializada = true;
                    using var cmdMark = new SqliteCommand("INSERT OR IGNORE INTO AppMetadata (Clave, Valor) VALUES ('SemillaInicializada', 'true');", conexion);
                    cmdMark.ExecuteNonQuery();
                }
            }
        }
        catch { }

        if (!semillaInicializada)
        {
            string seedData = @"
                INSERT OR IGNORE INTO Usuarios (IdUsuario, Nombre, NombreUsuario, Password, Rol)
                VALUES (1, 'Administrador', 'admin', 'admin123', 'Admin');

                INSERT OR IGNORE INTO Servicios (IdServicio, Nombre, Descripcion, Precio, TiempoEstimado, UnidadMedida) VALUES 
                (1, 'Lavado General', 'Lavado de ropa por kilogramo', 15.00, 120, 'Kg'),
                (2, 'Secado', 'Secado de ropa por carga', 30.00, 60, 'Carga'),
                (3, 'Planchado', 'Planchado por prenda (Opcional)', 10.00, 15, 'Unidad');

                INSERT OR IGNORE INTO AppMetadata (Clave, Valor) VALUES ('SemillaInicializada', 'true');
            ";

            using (var cmd = new SqliteCommand(seedData, conexion))
            {
                cmd.ExecuteNonQuery();
            }
        }
    }
}
