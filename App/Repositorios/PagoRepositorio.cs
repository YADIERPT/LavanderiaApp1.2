using Microsoft.Data.Sqlite;
using LavanderiaApp.Modelos;
using System.Collections.Generic;

namespace LavanderiaApp.Repositorios;

public class PagoRepositorio
{
    public void Guardar(Pago pago)
    {
        using var conexion = new SqliteConnection(Config.ConnectionString);
        conexion.Open();

        string query = @"
            INSERT INTO Pagos (IdPedido, MetodoPago, FechaPago, MontoPago)
            VALUES (@IdPedido, @MetodoPago, @FechaPago, @MontoPago)";
        using var command = new SqliteCommand(query, conexion);
        command.Parameters.AddWithValue("@IdPedido", pago.IdPedido);
        command.Parameters.AddWithValue("@MetodoPago", pago.Metodo.ToString());
        command.Parameters.AddWithValue("@FechaPago", pago.FechaPago.ToString("yyyy-MM-dd HH:mm:ss"));
        command.Parameters.AddWithValue("@MontoPago", pago.MontoPago);
        command.ExecuteNonQuery();
    }

    public List<Pago> ObtenerTodos()
    {
        var lista = new List<Pago>();
        using var conexion = new SqliteConnection(Config.ConnectionString);
        conexion.Open();

        string query = "SELECT IdPago, IdPedido, MetodoPago, FechaPago, MontoPago FROM Pagos";
        using var command = new SqliteCommand(query, conexion);
        using var reader = command.ExecuteReader();

        while (reader.Read())
        {
            string metodoStr = reader.GetString(2);
            Pago.MetodoPago enumMetodo = System.Enum.TryParse<Pago.MetodoPago>(metodoStr, out var res) ? res : Pago.MetodoPago.Efectivo;

            lista.Add(new Pago
            {
                IdPago = reader.GetInt32(0),
                IdPedido = reader.GetInt32(1),
                Metodo = enumMetodo,
                FechaPago = System.DateTime.Parse(reader.GetString(3)),
                MontoPago = reader.GetDecimal(4)
            });
        }
        return lista;
    }
}