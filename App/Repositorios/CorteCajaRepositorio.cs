using Microsoft.Data.Sqlite;
using LavanderiaApp.Modelos;
using System;
using System.Collections.Generic;

namespace LavanderiaApp.Repositorios;

public class CorteCajaRepositorio
{
    public void Guardar(CorteCaja corte)
    {
        using var conexion = new SqliteConnection(Config.ConnectionString);
        conexion.Open();

        string query = @"
            INSERT INTO CortesCaja (Fecha, EfectivoReportado, EfectivoEsperado, Diferencia, Empleado)
            VALUES (@Fecha, @EfectivoReportado, @EfectivoEsperado, @Diferencia, @Empleado)";
        using var command = new SqliteCommand(query, conexion);
        command.Parameters.AddWithValue("@Fecha", corte.Fecha.ToString("yyyy-MM-dd HH:mm:ss"));
        command.Parameters.AddWithValue("@EfectivoReportado", corte.EfectivoReportado);
        command.Parameters.AddWithValue("@EfectivoEsperado", corte.EfectivoEsperado);
        command.Parameters.AddWithValue("@Diferencia", corte.Diferencia);
        command.Parameters.AddWithValue("@Empleado", corte.Empleado);
        command.ExecuteNonQuery();
    }

    public List<CorteCaja> ObtenerTodos()
    {
        var lista = new List<CorteCaja>();
        using var conexion = new SqliteConnection(Config.ConnectionString);
        conexion.Open();

        string query = "SELECT IdCorte, Fecha, EfectivoReportado, EfectivoEsperado, Diferencia, Empleado FROM CortesCaja";
        using var command = new SqliteCommand(query, conexion);
        using var reader = command.ExecuteReader();

        while (reader.Read())
        {
            lista.Add(new CorteCaja
            {
                IdCorte = reader.GetInt32(0),
                Fecha = DateTime.Parse(reader.GetString(1)),
                EfectivoReportado = reader.GetDecimal(2),
                EfectivoEsperado = reader.GetDecimal(3),
                Diferencia = reader.GetDecimal(4),
                Empleado = reader.GetString(5)
            });
        }
        return lista;
    }
}
