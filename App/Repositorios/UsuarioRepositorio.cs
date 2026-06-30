using Microsoft.Data.Sqlite;
using LavanderiaApp.Modelos;

namespace LavanderiaApp.Repositorios;

public class UsuarioRepositorio
{
    public Usuario ObtenerPorNombreUsuario(string nombreUsuario)
    {
        using var conexion = new SqliteConnection(Config.ConnectionString);
        conexion.Open();

        string query = "SELECT * FROM Usuarios WHERE NombreUsuario = @NombreUsuario";
        using var command = new SqliteCommand(query, conexion);
        command.Parameters.AddWithValue("@NombreUsuario", nombreUsuario);

        using var reader = command.ExecuteReader();
        if (reader.Read())
        {
            return new Usuario
            {
                IdUsuario = reader.GetInt32(reader.GetOrdinal("IdUsuario")),
                Nombre = reader.GetString(reader.GetOrdinal("Nombre")),
                NombreUsuario = reader.GetString(reader.GetOrdinal("NombreUsuario")),
                Password = reader.GetString(reader.GetOrdinal("Password")),
                Rol = reader.GetString(reader.GetOrdinal("Rol"))
            };
        }
        return null;
    }
}