using LavanderiaApp.Modelos;
using Microsoft.Data.Sqlite;

namespace LavanderiaApp;

public class ClienteRepositorio
{
    public void Guardar(Cliente cliente)
    {
        using var conexion = new SqliteConnection(Config.ConnectionString);

        conexion.Open();

        string query =
            @"INSERT INTO Clientes
        (Nombre,Telefono,Direccion)

        VALUES

        (@Nombre,@Telefono,@Direccion)";

        using var cmd =
            new SqliteCommand(
                query,
                conexion);

        cmd.Parameters.AddWithValue(
            "@Nombre",
            cliente.Nombre);

        cmd.Parameters.AddWithValue(
            "@Telefono",
            cliente.Telefono);

        cmd.Parameters.AddWithValue(
            "@Direccion",
            cliente.Direccion);

        cmd.ExecuteNonQuery();
    }

    public System.Collections.Generic.List<Cliente> ListarTodo()
    {
        var lista = new System.Collections.Generic.List<Cliente>();
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
}