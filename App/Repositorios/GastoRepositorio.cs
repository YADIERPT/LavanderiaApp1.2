using Microsoft.Data.Sqlite;
using LavanderiaApp.Modelos;
using System;
using System.Collections.Generic;

namespace LavanderiaApp.Repositorios;

public class GastoRepositorio
{
    public void Guardar(Gasto gasto)
    {
        using var conexion = new SqliteConnection(Config.ConnectionString);
        conexion.Open();

        string query = @"
            INSERT INTO Gastos (Monto, Concepto, Fecha)
            VALUES (@Monto, @Concepto, @Fecha)";
        using var command = new SqliteCommand(query, conexion);
        command.Parameters.AddWithValue("@Monto", gasto.Monto);
        command.Parameters.AddWithValue("@Concepto", gasto.Concepto);
        command.Parameters.AddWithValue("@Fecha", gasto.Fecha.ToString("yyyy-MM-dd HH:mm:ss"));
        command.ExecuteNonQuery();
    }

    public List<Gasto> ObtenerTodos()
    {
        var lista = new List<Gasto>();
        using var conexion = new SqliteConnection(Config.ConnectionString);
        conexion.Open();

        string query = "SELECT IdGasto, Monto, Concepto, Fecha FROM Gastos";
        using var command = new SqliteCommand(query, conexion);
        using var reader = command.ExecuteReader();

        while (reader.Read())
        {
            lista.Add(new Gasto
            {
                IdGasto = reader.GetInt32(0),
                Monto = reader.GetDecimal(1),
                Concepto = reader.GetString(2),
                Fecha = DateTime.Parse(reader.GetString(3))
            });
        }
        return lista;
    }
}
