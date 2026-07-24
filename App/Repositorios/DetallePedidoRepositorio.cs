using Microsoft.Data.Sqlite;
using LavanderiaApp.Modelos;
using System.Collections.Generic;

namespace LavanderiaApp.Repositorios;

public class DetallePedidoRepositorio : IDetallePedidoRepositorio
{
    public void Guardar(DetallePedido detalle)
    {
        using var conexion = new SqliteConnection(Config.ConnectionString);
        conexion.Open();

        string query = @"
            INSERT INTO DetallesPedido (IdPedido, IdServicio, Cantidad, PrecioUnitario, Subtotal)
            VALUES (@IdPedido, @IdServicio, @Cantidad, @PrecioUnitario, @Subtotal)";
        using var command = new SqliteCommand(query, conexion);
        command.Parameters.AddWithValue("@IdPedido", detalle.IdPedido);
        command.Parameters.AddWithValue("@IdServicio", detalle.IdServicio);
        command.Parameters.AddWithValue("@Cantidad", detalle.Cantidad);
        command.Parameters.AddWithValue("@PrecioUnitario", detalle.PrecioUnitario);
        command.Parameters.AddWithValue("@Subtotal", detalle.Subtotal);
        command.ExecuteNonQuery();
    }

    public List<DetallePedido> ObtenerPorPedido(int idPedido)
    {
        var lista = new List<DetallePedido>();
        using var conexion = new SqliteConnection(Config.ConnectionString);
        conexion.Open();

        string query = "SELECT * FROM DetallesPedido WHERE IdPedido = @IdPedido";
        using var cmd = new SqliteCommand(query, conexion);
        cmd.Parameters.AddWithValue("@IdPedido", idPedido);
        using var reader = cmd.ExecuteReader();

        while (reader.Read())
        {
            lista.Add(new DetallePedido
            {
                IdDetallePedido = reader.GetInt32(reader.GetOrdinal("IdDetallePedido")),
                IdPedido = reader.GetInt32(reader.GetOrdinal("IdPedido")),
                IdServicio = reader.GetInt32(reader.GetOrdinal("IdServicio")),
                Cantidad = reader.GetDouble(reader.GetOrdinal("Cantidad")),
                PrecioUnitario = reader.GetDecimal(reader.GetOrdinal("PrecioUnitario")),
                Subtotal = reader.GetDecimal(reader.GetOrdinal("Subtotal"))
            });
        }
        return lista;
    }
}