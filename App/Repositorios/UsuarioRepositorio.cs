using Microsoft.Data.Sqlite;
using LavanderiaApp.Modelos;
using System.Collections.Generic;

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

    public Usuario ObtenerPorId(int idUsuario)
    {
        using var conexion = new SqliteConnection(Config.ConnectionString);
        conexion.Open();

        string query = "SELECT * FROM Usuarios WHERE IdUsuario = @IdUsuario";
        using var command = new SqliteCommand(query, conexion);
        command.Parameters.AddWithValue("@IdUsuario", idUsuario);

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

    public List<Usuario> ListarTodo()
    {
        var lista = new List<Usuario>();
        using var conexion = new SqliteConnection(Config.ConnectionString);
        conexion.Open();

        string query = "SELECT * FROM Usuarios";
        using var command = new SqliteCommand(query, conexion);

        using var reader = command.ExecuteReader();
        while (reader.Read())
        {
            lista.Add(new Usuario
            {
                IdUsuario = reader.GetInt32(reader.GetOrdinal("IdUsuario")),
                Nombre = reader.GetString(reader.GetOrdinal("Nombre")),
                NombreUsuario = reader.GetString(reader.GetOrdinal("NombreUsuario")),
                Password = reader.GetString(reader.GetOrdinal("Password")),
                Rol = reader.GetString(reader.GetOrdinal("Rol"))
            });
        }
        return lista;
    }

    public void Guardar(Usuario usuario)
    {
        using var conexion = new SqliteConnection(Config.ConnectionString);
        conexion.Open();

        string query = @"INSERT OR IGNORE INTO Usuarios (Nombre, NombreUsuario, Password, Rol) 
                         VALUES (@Nombre, @NombreUsuario, @Password, @Rol)";
        using var command = new SqliteCommand(query, conexion);
        command.Parameters.AddWithValue("@Nombre", usuario.Nombre);
        command.Parameters.AddWithValue("@NombreUsuario", usuario.NombreUsuario);
        command.Parameters.AddWithValue("@Password", usuario.Password);
        command.Parameters.AddWithValue("@Rol", string.IsNullOrEmpty(usuario.Rol) ? "Empleado" : usuario.Rol);

        command.ExecuteNonQuery();
    }

    public void Actualizar(Usuario usuario)
    {
        using var conexion = new SqliteConnection(Config.ConnectionString);
        conexion.Open();

        string query = @"UPDATE Usuarios 
                         SET Nombre = @Nombre, NombreUsuario = @NombreUsuario, Password = @Password, Rol = @Rol 
                         WHERE IdUsuario = @IdUsuario";

        using var command = new SqliteCommand(query, conexion);
        command.Parameters.AddWithValue("@IdUsuario", usuario.IdUsuario);
        command.Parameters.AddWithValue("@Nombre", usuario.Nombre);
        command.Parameters.AddWithValue("@NombreUsuario", usuario.NombreUsuario);
        command.Parameters.AddWithValue("@Password", usuario.Password);
        command.Parameters.AddWithValue("@Rol", string.IsNullOrEmpty(usuario.Rol) ? "Empleado" : usuario.Rol);

        command.ExecuteNonQuery();
    }

    public void Eliminar(int idUsuario)
    {
        using var conexion = new SqliteConnection(Config.ConnectionString);
        conexion.Open();

        string query = "DELETE FROM Usuarios WHERE IdUsuario = @IdUsuario";
        using var command = new SqliteCommand(query, conexion);
        command.Parameters.AddWithValue("@IdUsuario", idUsuario);

        command.ExecuteNonQuery();
    }
}