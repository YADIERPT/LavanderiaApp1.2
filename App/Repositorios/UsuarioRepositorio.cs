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
            return MapearUsuario(reader);
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
            return MapearUsuario(reader);
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
            lista.Add(MapearUsuario(reader));
        }
        return lista;
    }

    private Usuario MapearUsuario(SqliteDataReader reader)
    {
        string rol = reader.GetString(reader.GetOrdinal("Rol"));
        var u = LavanderiaApp.Servicios.UsuarioFactory.CrearUsuario(rol);
        
        u.IdUsuario = reader.GetInt32(reader.GetOrdinal("IdUsuario"));
        u.Nombre = reader.GetString(reader.GetOrdinal("Nombre"));
        u.NombreUsuario = reader.GetString(reader.GetOrdinal("NombreUsuario"));
        u.Password = reader.GetString(reader.GetOrdinal("Password"));
        u.Rol = rol;


        try { u.Telefono = !reader.IsDBNull(reader.GetOrdinal("Telefono")) ? reader.GetString(reader.GetOrdinal("Telefono")) : ""; } catch { }
        try { u.Correo = !reader.IsDBNull(reader.GetOrdinal("Correo")) ? reader.GetString(reader.GetOrdinal("Correo")) : ""; } catch { }
        try { u.Turno = !reader.IsDBNull(reader.GetOrdinal("Turno")) ? reader.GetString(reader.GetOrdinal("Turno")) : ""; } catch { }
        try { u.FechaContrato = !reader.IsDBNull(reader.GetOrdinal("FechaContrato")) ? reader.GetString(reader.GetOrdinal("FechaContrato")) : ""; } catch { }
        try { u.Salario = !reader.IsDBNull(reader.GetOrdinal("Salario")) ? reader.GetDecimal(reader.GetOrdinal("Salario")) : 0m; } catch { }
        try { u.Sucursal = !reader.IsDBNull(reader.GetOrdinal("Sucursal")) ? reader.GetString(reader.GetOrdinal("Sucursal")) : ""; } catch { }
        try { u.Edad = !reader.IsDBNull(reader.GetOrdinal("Edad")) ? reader.GetInt32(reader.GetOrdinal("Edad")) : 0; } catch { }

        return u;
    }

    public void Guardar(Usuario usuario)
    {
        using var conexion = new SqliteConnection(Config.ConnectionString);
        conexion.Open();

        string query = @"INSERT OR IGNORE INTO Usuarios (Nombre, NombreUsuario, Password, Rol, Telefono, Correo, Turno, FechaContrato, Salario, Sucursal, Edad) 
                         VALUES (@Nombre, @NombreUsuario, @Password, @Rol, @Telefono, @Correo, @Turno, @FechaContrato, @Salario, @Sucursal, @Edad)";
        using var command = new SqliteCommand(query, conexion);
        command.Parameters.AddWithValue("@Nombre", usuario.Nombre ?? "");
        command.Parameters.AddWithValue("@NombreUsuario", usuario.NombreUsuario ?? "");
        command.Parameters.AddWithValue("@Password", usuario.Password ?? "");
        command.Parameters.AddWithValue("@Rol", string.IsNullOrEmpty(usuario.Rol) ? "Empleado" : usuario.Rol);
        command.Parameters.AddWithValue("@Telefono", usuario.Telefono ?? "");
        command.Parameters.AddWithValue("@Correo", usuario.Correo ?? "");
        command.Parameters.AddWithValue("@Turno", usuario.Turno ?? "");
        command.Parameters.AddWithValue("@FechaContrato", usuario.FechaContrato ?? "");
        command.Parameters.AddWithValue("@Salario", usuario.Salario);
        command.Parameters.AddWithValue("@Sucursal", usuario.Sucursal ?? "");
        command.Parameters.AddWithValue("@Edad", usuario.Edad);

        command.ExecuteNonQuery();
    }

    public void Actualizar(Usuario usuario)
    {
        using var conexion = new SqliteConnection(Config.ConnectionString);
        conexion.Open();

        string query = @"UPDATE Usuarios 
                         SET Nombre = @Nombre, NombreUsuario = @NombreUsuario, Password = @Password, Rol = @Rol,
                             Telefono = @Telefono, Correo = @Correo, Turno = @Turno, FechaContrato = @FechaContrato,
                             Salario = @Salario, Sucursal = @Sucursal, Edad = @Edad
                         WHERE IdUsuario = @IdUsuario";

        using var command = new SqliteCommand(query, conexion);
        command.Parameters.AddWithValue("@IdUsuario", usuario.IdUsuario);
        command.Parameters.AddWithValue("@Nombre", usuario.Nombre ?? "");
        command.Parameters.AddWithValue("@NombreUsuario", usuario.NombreUsuario ?? "");
        command.Parameters.AddWithValue("@Password", usuario.Password ?? "");
        command.Parameters.AddWithValue("@Rol", string.IsNullOrEmpty(usuario.Rol) ? "Empleado" : usuario.Rol);
        command.Parameters.AddWithValue("@Telefono", usuario.Telefono ?? "");
        command.Parameters.AddWithValue("@Correo", usuario.Correo ?? "");
        command.Parameters.AddWithValue("@Turno", usuario.Turno ?? "");
        command.Parameters.AddWithValue("@FechaContrato", usuario.FechaContrato ?? "");
        command.Parameters.AddWithValue("@Salario", usuario.Salario);
        command.Parameters.AddWithValue("@Sucursal", usuario.Sucursal ?? "");
        command.Parameters.AddWithValue("@Edad", usuario.Edad);

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