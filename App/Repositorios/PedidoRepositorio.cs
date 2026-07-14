using Microsoft.Data.Sqlite;
using LavanderiaApp.Modelos;

namespace  LavanderiaApp;

public class PedidoRepositorio
{
    public int Guardar(Pedido pedido)
    {
        using var conexion = new SqliteConnection(Config.ConnectionString);
        conexion.Open();

        string query = @"
            INSERT INTO Pedidos (IdCliente, IdUsuario, FechaRecepcion, FechaEntrega, Estado, Total, InventarioRestado, CostoInsumos, MaquinaAsignada)
            VALUES (@IdCliente, @IdUsuario, @FechaRecepcion, @FechaEntrega, @Estado, @Total, @InventarioRestado, @CostoInsumos, @MaquinaAsignada);
            SELECT last_insert_rowid();";
        using var command = new SqliteCommand(query, conexion);
        command.Parameters.AddWithValue("@IdCliente", pedido.IdCliente);
        command.Parameters.AddWithValue("@IdUsuario", pedido.IdUsuario);
        command.Parameters.AddWithValue("@FechaRecepcion", pedido.FechaRecepcion.ToString("yyyy-MM-dd HH:mm:ss"));
        command.Parameters.AddWithValue("@FechaEntrega", pedido.FechaEntrega.HasValue ? pedido.FechaEntrega.Value.ToString("yyyy-MM-dd HH:mm:ss") : (object)System.DBNull.Value);
        command.Parameters.AddWithValue("@Estado", string.IsNullOrEmpty(pedido.Estado) ? "En espera" : pedido.Estado);
        command.Parameters.AddWithValue("@Total", pedido.Total);
        command.Parameters.AddWithValue("@InventarioRestado", pedido.InventarioRestado);
        command.Parameters.AddWithValue("@CostoInsumos", pedido.CostoInsumos);
        command.Parameters.AddWithValue("@MaquinaAsignada", pedido.MaquinaAsignada ?? "");
        
        return (int)(long)command.ExecuteScalar();
    }
    
    public System.Collections.Generic.List<Pedido> ObtenerTodos()
    {
        var lista = new System.Collections.Generic.List<Pedido>();
        using var conexion = new SqliteConnection(Config.ConnectionString);
        conexion.Open();

        string query = "SELECT * FROM Pedidos";
        using var cmd = new SqliteCommand(query, conexion);
        using var reader = cmd.ExecuteReader();

        while (reader.Read())
        {
            lista.Add(new Pedido
            {
                IdPedido = reader.GetInt32(reader.GetOrdinal("IdPedido")),
                IdCliente = reader.GetInt32(reader.GetOrdinal("IdCliente")),
                IdUsuario = reader.GetInt32(reader.GetOrdinal("IdUsuario")),
                FechaRecepcion = System.DateTime.Parse(reader.GetString(reader.GetOrdinal("FechaRecepcion")), System.Globalization.CultureInfo.InvariantCulture),
                FechaEntrega = reader.IsDBNull(reader.GetOrdinal("FechaEntrega")) ? (System.DateTime?)null : System.DateTime.Parse(reader.GetString(reader.GetOrdinal("FechaEntrega")), System.Globalization.CultureInfo.InvariantCulture),
                Estado = reader.GetString(reader.GetOrdinal("Estado")),
                Total = reader.GetDecimal(reader.GetOrdinal("Total")),
                InventarioRestado = reader.GetInt32(reader.GetOrdinal("InventarioRestado")),
                CostoInsumos = reader.GetDecimal(reader.GetOrdinal("CostoInsumos")),
                MaquinaAsignada = reader.IsDBNull(reader.GetOrdinal("MaquinaAsignada")) ? "" : reader.GetString(reader.GetOrdinal("MaquinaAsignada"))
            });
            
        }
        return lista;
    }

    public Pedido ObtenerPorId(int idPedido)
    {
        using var conexion = new SqliteConnection(Config.ConnectionString);
        conexion.Open();

        string query = "SELECT * FROM Pedidos WHERE IdPedido = @IdPedido";
        using var cmd = new SqliteCommand(query, conexion);
        cmd.Parameters.AddWithValue("@IdPedido", idPedido);
        using var reader = cmd.ExecuteReader();

        if (reader.Read())
        {
            return new Pedido
            {
                IdPedido = reader.GetInt32(reader.GetOrdinal("IdPedido")),
                IdCliente = reader.GetInt32(reader.GetOrdinal("IdCliente")),
                IdUsuario = reader.GetInt32(reader.GetOrdinal("IdUsuario")),
                FechaRecepcion = System.DateTime.Parse(reader.GetString(reader.GetOrdinal("FechaRecepcion")), System.Globalization.CultureInfo.InvariantCulture),
                FechaEntrega = reader.IsDBNull(reader.GetOrdinal("FechaEntrega")) ? (System.DateTime?)null : System.DateTime.Parse(reader.GetString(reader.GetOrdinal("FechaEntrega")), System.Globalization.CultureInfo.InvariantCulture),
                Estado = reader.GetString(reader.GetOrdinal("Estado")),
                Total = reader.GetDecimal(reader.GetOrdinal("Total")),
                InventarioRestado = reader.GetInt32(reader.GetOrdinal("InventarioRestado")),
                CostoInsumos = reader.GetDecimal(reader.GetOrdinal("CostoInsumos")),
                MaquinaAsignada = reader.IsDBNull(reader.GetOrdinal("MaquinaAsignada")) ? "" : reader.GetString(reader.GetOrdinal("MaquinaAsignada"))
            };
        }
        return null;
    }

    public void Entregar(int idPedido)
    {
        using var conexion = new SqliteConnection(Config.ConnectionString);
        conexion.Open();

        string query = "UPDATE Pedidos SET FechaEntrega = @FechaEntrega, Estado = 'Entregado' WHERE IdPedido = @IdPedido";
        using var command = new SqliteCommand(query, conexion);
        command.Parameters.AddWithValue("@FechaEntrega", System.DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));
        command.Parameters.AddWithValue("@IdPedido", idPedido);
        command.ExecuteNonQuery();
    }

    public void ActualizarEstado(int idPedido, string nuevoEstado)
    {
        using var conexion = new SqliteConnection(Config.ConnectionString);
        conexion.Open();

        string query = "UPDATE Pedidos SET Estado = @Estado WHERE IdPedido = @IdPedido";
        using var command = new SqliteCommand(query, conexion);
        command.Parameters.AddWithValue("@Estado", nuevoEstado);
        command.Parameters.AddWithValue("@IdPedido", idPedido);
        command.ExecuteNonQuery();
    }

    public void ActualizarEstadoYMaquina(int idPedido, string nuevoEstado, string maquina)
    {
        using var conexion = new SqliteConnection(Config.ConnectionString);
        conexion.Open();

        string query = "UPDATE Pedidos SET Estado = @Estado, MaquinaAsignada = @Maquina WHERE IdPedido = @IdPedido";
        using var command = new SqliteCommand(query, conexion);
        command.Parameters.AddWithValue("@Estado", nuevoEstado);
        command.Parameters.AddWithValue("@Maquina", maquina ?? "");
        command.Parameters.AddWithValue("@IdPedido", idPedido);
        command.ExecuteNonQuery();
    }

    public void ActualizarInventarioRestado(int idPedido, int restado, decimal costo)
    {
        using var conexion = new SqliteConnection(Config.ConnectionString);
        conexion.Open();

        string query = "UPDATE Pedidos SET InventarioRestado = @Restado, CostoInsumos = @Costo WHERE IdPedido = @IdPedido";
        using var command = new SqliteCommand(query, conexion);
        command.Parameters.AddWithValue("@Restado", restado);
        command.Parameters.AddWithValue("@Costo", costo);
        command.Parameters.AddWithValue("@IdPedido", idPedido);
        command.ExecuteNonQuery();
    }
}
