using System;
using System.Collections.Generic;
using Microsoft.Data.Sqlite;
using LavanderiaApp.Modelos;

namespace LavanderiaApp.Repositorios;

public class NotificacionRepositorio
{
    public void Agregar(Notificacion notificacion)
    {
        try
        {
            using var conexion = new SqliteConnection(Config.ConnectionString);
            conexion.Open();

            string query = @"INSERT INTO Notificaciones (Titulo, Mensaje, Tipo, Fecha, Leida) 
                             VALUES (@Titulo, @Mensaje, @Tipo, @Fecha, @Leida)";
            using var cmd = new SqliteCommand(query, conexion);
            cmd.Parameters.AddWithValue("@Titulo", notificacion.Titulo ?? "");
            cmd.Parameters.AddWithValue("@Mensaje", notificacion.Mensaje ?? "");
            cmd.Parameters.AddWithValue("@Tipo", notificacion.Tipo ?? "Alerta");
            cmd.Parameters.AddWithValue("@Fecha", notificacion.Fecha.ToString("yyyy-MM-dd HH:mm:ss"));
            cmd.Parameters.AddWithValue("@Leida", notificacion.Leida ? 1 : 0);

            cmd.ExecuteNonQuery();
        }
        catch { }
    }

    public List<Notificacion> ObtenerTodas()
    {
        var lista = new List<Notificacion>();
        try
        {
            using var conexion = new SqliteConnection(Config.ConnectionString);
            conexion.Open();

            string query = "SELECT IdNotificacion, Titulo, Mensaje, Tipo, Fecha, Leida FROM Notificaciones ORDER BY IdNotificacion DESC";
            using var cmd = new SqliteCommand(query, conexion);
            using var reader = cmd.ExecuteReader();
            while (reader.Read())
            {
                var fechaStr = reader.GetValue(4)?.ToString() ?? "";
                DateTime.TryParse(fechaStr, out var fecha);
                if (fecha == DateTime.MinValue) fecha = DateTime.Now;

                lista.Add(new Notificacion
                {
                    IdNotificacion = Convert.ToInt32(reader.GetValue(0)),
                    Titulo = reader.GetValue(1)?.ToString() ?? "",
                    Mensaje = reader.GetValue(2)?.ToString() ?? "",
                    Tipo = reader.GetValue(3)?.ToString() ?? "Alerta",
                    Fecha = fecha,
                    Leida = Convert.ToInt32(reader.GetValue(5)) == 1
                });
            }
        }
        catch { }
        return lista;
    }

    public void MarcarComoLeidas()
    {
        try
        {
            using var conexion = new SqliteConnection(Config.ConnectionString);
            conexion.Open();

            string query = "UPDATE Notificaciones SET Leida = 1";
            using var cmd = new SqliteCommand(query, conexion);
            cmd.ExecuteNonQuery();
        }
        catch { }
    }
}
