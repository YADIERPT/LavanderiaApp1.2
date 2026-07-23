using Microsoft.Data.Sqlite;

namespace LavanderiaApp.Repositorios;

public class InsumoItem
{
    public int IdInsumo { get; set; }
    public string Nombre { get; set; } = "";
    public string Categoria { get; set; } = "Detergentes";
    public double Cantidad { get; set; }
    public string UnidadMedida { get; set; } = "Litros";
    public double StockMinimo { get; set; }
    public decimal PrecioUnitario { get; set; }
    public string UltimaActualizacion { get; set; } = DateTime.Now.ToString("yyyy-MM-dd");
    public double CapacidadEnvase { get; set; } = 0.0;

    public bool EstaEnAlertaStock => Cantidad <= StockMinimo;
    public decimal ValuacionTotal => CapacidadEnvase > 0 ? (decimal)(Cantidad / CapacidadEnvase) * PrecioUnitario : (decimal)Cantidad * PrecioUnitario;
}

public class InventarioRepositorio
{
    public List<InsumoItem> ObtenerTodos()
    {
        var lista = new List<InsumoItem>();
        using var conexion = new SqliteConnection(Config.ConnectionString);
        conexion.Open();

        string query = "SELECT * FROM Inventario";
        using var cmd = new SqliteCommand(query, conexion);
        using var reader = cmd.ExecuteReader();

        while (reader.Read())
        {
            double cap = 0;
            try { cap = reader.GetDouble(reader.GetOrdinal("CapacidadEnvase")); } catch { cap = 0; }
            
            lista.Add(new InsumoItem
            {
                IdInsumo = reader.GetInt32(reader.GetOrdinal("IdInsumo")),
                Nombre = reader.GetString(reader.GetOrdinal("Nombre")),
                Categoria = reader.GetString(reader.GetOrdinal("Categoria")),
                Cantidad = reader.GetDouble(reader.GetOrdinal("Cantidad")),
                UnidadMedida = reader.GetString(reader.GetOrdinal("UnidadMedida")),
                StockMinimo = reader.GetDouble(reader.GetOrdinal("StockMinimo")),
                PrecioUnitario = reader.GetDecimal(reader.GetOrdinal("PrecioUnitario")),
                UltimaActualizacion = reader.GetString(reader.GetOrdinal("UltimaActualizacion")),
                CapacidadEnvase = cap
            });
        }
        return lista;
    }

    public void Guardar(InsumoItem item)
    {
        using var conexion = new SqliteConnection(Config.ConnectionString);
        conexion.Open();

        if (item.IdInsumo > 0)
        {
            string updateQuery = @"
                UPDATE Inventario SET 
                Nombre = @Nombre, Categoria = @Categoria, Cantidad = @Cantidad, 
                UnidadMedida = @UnidadMedida, StockMinimo = @StockMinimo, 
                PrecioUnitario = @PrecioUnitario, UltimaActualizacion = @UltimaActualizacion, CapacidadEnvase = @CapacidadEnvase
                WHERE IdInsumo = @IdInsumo";
            using var cmd = new SqliteCommand(updateQuery, conexion);
            cmd.Parameters.AddWithValue("@Nombre", item.Nombre);
            cmd.Parameters.AddWithValue("@Categoria", item.Categoria);
            cmd.Parameters.AddWithValue("@Cantidad", item.Cantidad);
            cmd.Parameters.AddWithValue("@UnidadMedida", item.UnidadMedida);
            cmd.Parameters.AddWithValue("@StockMinimo", item.StockMinimo);
            cmd.Parameters.AddWithValue("@PrecioUnitario", item.PrecioUnitario);
            cmd.Parameters.AddWithValue("@UltimaActualizacion", DateTime.Now.ToString("yyyy-MM-dd"));
            cmd.Parameters.AddWithValue("@CapacidadEnvase", item.CapacidadEnvase);
            cmd.Parameters.AddWithValue("@IdInsumo", item.IdInsumo);
            cmd.ExecuteNonQuery();
        }
        else
        {
            string insertQuery = @"
                INSERT INTO Inventario (Nombre, Categoria, Cantidad, UnidadMedida, StockMinimo, PrecioUnitario, UltimaActualizacion, CapacidadEnvase)
                VALUES (@Nombre, @Categoria, @Cantidad, @UnidadMedida, @StockMinimo, @PrecioUnitario, @UltimaActualizacion, @CapacidadEnvase)";
            using var cmd = new SqliteCommand(insertQuery, conexion);
            cmd.Parameters.AddWithValue("@Nombre", item.Nombre);
            cmd.Parameters.AddWithValue("@Categoria", item.Categoria);
            cmd.Parameters.AddWithValue("@Cantidad", item.Cantidad);
            cmd.Parameters.AddWithValue("@UnidadMedida", item.UnidadMedida);
            cmd.Parameters.AddWithValue("@StockMinimo", item.StockMinimo);
            cmd.Parameters.AddWithValue("@PrecioUnitario", item.PrecioUnitario);
            cmd.Parameters.AddWithValue("@UltimaActualizacion", DateTime.Now.ToString("yyyy-MM-dd"));
            cmd.Parameters.AddWithValue("@CapacidadEnvase", item.CapacidadEnvase);
            cmd.ExecuteNonQuery();
        }
    }

    public void ActualizarStock(int idInsumo, double nuevaCantidad)
    {
        using var conexion = new SqliteConnection(Config.ConnectionString);
        conexion.Open();

        string query = "UPDATE Inventario SET Cantidad = @Cantidad, UltimaActualizacion = @Fecha WHERE IdInsumo = @IdInsumo";
        using var cmd = new SqliteCommand(query, conexion);
        cmd.Parameters.AddWithValue("@Cantidad", nuevaCantidad);
        cmd.Parameters.AddWithValue("@Fecha", DateTime.Now.ToString("yyyy-MM-dd"));
        cmd.Parameters.AddWithValue("@IdInsumo", idInsumo);
        cmd.ExecuteNonQuery();
    }

    public void Eliminar(int idInsumo)
    {
        using var conexion = new SqliteConnection(Config.ConnectionString);
        conexion.Open();

        using var transaccion = conexion.BeginTransaction();
        try
        {
            // Primero eliminamos cualquier receta vinculada a este insumo
            string queryRecetas = "DELETE FROM Recetas WHERE IdInsumo = @IdInsumo";
            using (var cmdRecetas = new SqliteCommand(queryRecetas, conexion, transaccion))
            {
                cmdRecetas.Parameters.AddWithValue("@IdInsumo", idInsumo);
                cmdRecetas.ExecuteNonQuery();
            }

            // Luego eliminamos el insumo
            string queryInsumo = "DELETE FROM Inventario WHERE IdInsumo = @IdInsumo";
            using (var cmdInsumo = new SqliteCommand(queryInsumo, conexion, transaccion))
            {
                cmdInsumo.Parameters.AddWithValue("@IdInsumo", idInsumo);
                cmdInsumo.ExecuteNonQuery();
            }

            transaccion.Commit();
        }
        catch
        {
            transaccion.Rollback();
            throw;
        }
    }
}
