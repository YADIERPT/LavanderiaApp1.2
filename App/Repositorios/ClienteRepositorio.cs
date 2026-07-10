using LavanderiaApp.Modelos;
using Microsoft.Data.Sqlite;
using System.Collections.Generic;

namespace LavanderiaApp;

public class ClienteRepositorio
{
    public void Guardar(Cliente cliente)
    {
        using var conexion = new SqliteConnection(Config.ConnectionString);
        conexion.Open();

        string query = @"INSERT INTO Clientes (Nombre, Telefono, Direccion)
                         VALUES (@Nombre, @Telefono, @Direccion)";

        using var cmd = new SqliteCommand(query, conexion);
        cmd.Parameters.AddWithValue("@Nombre", cliente.Nombre);
        cmd.Parameters.AddWithValue("@Telefono", cliente.Telefono ?? "");
        cmd.Parameters.AddWithValue("@Direccion", cliente.Direccion ?? "");

        cmd.ExecuteNonQuery();
    }

    public List<Cliente> ListarTodo()
    {
        var lista = new List<Cliente>();
        using var conexion = new SqliteConnection(Config.ConnectionString);
        conexion.Open();

        string query = "SELECT IdCliente, Nombre, Telefono, Direccion FROM Clientes";
        using var cmd = new SqliteCommand(query, conexion);
        using var reader = cmd.ExecuteReader();

        while (reader.Read())
        {
            lista.Add(new Cliente
            {
                IdCliente = reader.GetInt32(0),
                Nombre = reader.GetString(1),
                Telefono = reader.IsDBNull(2) ? "" : reader.GetString(2),
                Direccion = reader.IsDBNull(3) ? "" : reader.GetString(3)
            });
        }
        return lista;
    }

    public Cliente? ObtenerPorId(int idCliente)
    {
        using var conexion = new SqliteConnection(Config.ConnectionString);
        conexion.Open();

        string query = "SELECT IdCliente, Nombre, Telefono, Direccion FROM Clientes WHERE IdCliente = @IdCliente";
        using var cmd = new SqliteCommand(query, conexion);
        cmd.Parameters.AddWithValue("@IdCliente", idCliente);
        using var reader = cmd.ExecuteReader();

        if (reader.Read())
        {
            return new Cliente
            {
                IdCliente = reader.GetInt32(0),
                Nombre = reader.GetString(1),
                Telefono = reader.IsDBNull(2) ? "" : reader.GetString(2),
                Direccion = reader.IsDBNull(3) ? "" : reader.GetString(3)
            };
        }
        return null;
    }

    public void Actualizar(Cliente cliente)
    {
        using var conexion = new SqliteConnection(Config.ConnectionString);
        conexion.Open();

        string query = @"UPDATE Clientes 
                         SET Nombre = @Nombre, Telefono = @Telefono, Direccion = @Direccion 
                         WHERE IdCliente = @IdCliente";

        using var cmd = new SqliteCommand(query, conexion);
        cmd.Parameters.AddWithValue("@IdCliente", cliente.IdCliente);
        cmd.Parameters.AddWithValue("@Nombre", cliente.Nombre);
        cmd.Parameters.AddWithValue("@Telefono", cliente.Telefono ?? "");
        cmd.Parameters.AddWithValue("@Direccion", cliente.Direccion ?? "");

        cmd.ExecuteNonQuery();
    }

    public void Eliminar(int idCliente)
    {
        using var conexion = new SqliteConnection(Config.ConnectionString);
        conexion.Open();

        string query = "DELETE FROM Clientes WHERE IdCliente = @IdCliente";
        using var cmd = new SqliteCommand(query, conexion);
        cmd.Parameters.AddWithValue("@IdCliente", idCliente);

        cmd.ExecuteNonQuery();
    }
}