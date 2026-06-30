using Microsoft.Data.Sqlite;
using LavanderiaApp.Modelos;
using System.Collections.Generic;

namespace LavanderiaApp.Repositorios;

public class ServicioRepositorio
{
    public List<Servicio> ObtenerTodos()
    {
        var lista = new List<Servicio>();
        using var conexion = new SqliteConnection(Config.ConnectionString);
        conexion.Open();

        string query = "SELECT * FROM Servicios";
        using var cmd = new SqliteCommand(query, conexion);
        using var reader = cmd.ExecuteReader();

        while (reader.Read())
        {
            lista.Add(new Servicio
            {
                IdServicio = reader.GetInt32(reader.GetOrdinal("IdServicio")),
                Nombre = reader.GetString(reader.GetOrdinal("Nombre")),
                Descripcion = reader.IsDBNull(reader.GetOrdinal("Descripcion")) ? null : reader.GetString(reader.GetOrdinal("Descripcion")),
                Precio = reader.GetDecimal(reader.GetOrdinal("Precio")),
                TiempoEstimado = reader.GetInt32(reader.GetOrdinal("TiempoEstimado")),
                UnidadMedida = reader.GetString(reader.GetOrdinal("UnidadMedida"))
            });
        }
        return lista;
    }
}