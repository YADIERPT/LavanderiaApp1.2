using Microsoft.Data.Sqlite;
using LavanderiaApp.Modelos;

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
}