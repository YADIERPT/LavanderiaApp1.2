using System;
using System.Collections.Generic;
using Microsoft.Data.Sqlite;
using LavanderiaApp.Modelos;

namespace LavanderiaApp.Repositorios;

public class AuditoriaRepositorio
{
    public void Guardar(Auditoria auditoria)
    {
        using var conexion = new SqliteConnection(Config.ConnectionString);
        conexion.Open();

        string query = @"
            INSERT INTO Auditorias (Fecha, Usuario, Modulo, Accion, Detalle)
            VALUES (@Fecha, @Usuario, @Modulo, @Accion, @Detalle)
        ";

        using var cmd = new SqliteCommand(query, conexion);
        cmd.Parameters.AddWithValue("@Fecha", auditoria.Fecha.ToString("yyyy-MM-dd HH:mm:ss"));
        cmd.Parameters.AddWithValue("@Usuario", auditoria.Usuario);
        cmd.Parameters.AddWithValue("@Modulo", auditoria.Modulo);
        cmd.Parameters.AddWithValue("@Accion", auditoria.Accion);
        cmd.Parameters.AddWithValue("@Detalle", auditoria.Detalle);

        cmd.ExecuteNonQuery();
    }

    public List<Auditoria> ObtenerTodos()
    {
        var list = new List<Auditoria>();
        using var conexion = new SqliteConnection(Config.ConnectionString);
        conexion.Open();

        string query = "SELECT Id, Fecha, Usuario, Modulo, Accion, Detalle FROM Auditorias ORDER BY Fecha DESC";

        using var cmd = new SqliteCommand(query, conexion);
        using var reader = cmd.ExecuteReader();

        while (reader.Read())
        {
            list.Add(new Auditoria
            {
                Id = reader.GetInt32(0),
                Fecha = DateTime.TryParse(reader.GetString(1), out var f) ? f : DateTime.Now,
                Usuario = reader.GetString(2),
                Modulo = reader.GetString(3),
                Accion = reader.GetString(4),
                Detalle = reader.GetString(5)
            });
        }

        return list;
    }
}
