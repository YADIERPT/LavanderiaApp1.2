using Microsoft.Data.Sqlite;

namespace LavanderiaApp;

public class DatabaseInitializer
{
    public static void Inicializar()
    {
        using var conexion = new SqliteConnection(Config.ConnectionString);
        conexion.Open();

        string query = @"
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

            INSERT OR IGNORE INTO Usuarios (IdUsuario, Nombre, NombreUsuario, Password, Rol)
            VALUES (1, 'Administrador', 'admin', 'admin123', 'Admin');

            CREATE TABLE IF NOT EXISTS Pedidos (
                IdPedido INTEGER PRIMARY KEY AUTOINCREMENT,
                IdCliente INTEGER NOT NULL,
                IdUsuario INTEGER NOT NULL,
                FechaRecepcion TEXT NOT NULL,
                FechaEntrega TEXT,
                Estado TEXT NOT NULL,
                Total DECIMAL NOT NULL,
                FOREIGN KEY (IdCliente) REFERENCES Clientes(IdCliente),
                FOREIGN KEY (IdUsuario) REFERENCES Usuarios(IdUsuario)
            );
        ";

        using var cmd = new SqliteCommand(query, conexion);
        cmd.ExecuteNonQuery();

        // Migración rápida: Agregar columna 'Estado' si la tabla ya existía sin ella
        try
        {
            using var cmdAlter = new SqliteCommand("ALTER TABLE Pedidos ADD COLUMN Estado TEXT NOT NULL DEFAULT 'En espera';", conexion);
            cmdAlter.ExecuteNonQuery();
        }
        catch { /* La columna ya existe o la tabla se acaba de crear con ella */ }
    }
}
