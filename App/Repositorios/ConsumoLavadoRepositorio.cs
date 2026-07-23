using System;
using System.Collections.Generic;
using Microsoft.Data.Sqlite;

namespace LavanderiaApp.Modelos
{
    public class ConsumoLavado
    {
        public int IdConsumo { get; set; }
        public int IdInsumo { get; set; }
        public double Cantidad { get; set; }
        public string UnidadConsumo { get; set; } = "";
        
        // Propiedades de navegación (solo para UI)
        public string NombreInsumo { get; set; } = "";
        public string UnidadMedida { get; set; } = "";
    }
}

namespace LavanderiaApp.Repositorios
{
    using LavanderiaApp.Modelos;

    public class ConsumoLavadoRepositorio
    {
        public List<ConsumoLavado> ObtenerTodos()
        {
            var lista = new List<ConsumoLavado>();
            using var conexion = new SqliteConnection(Config.ConnectionString);
            conexion.Open();

            string query = @"
                SELECT c.IdConsumo, c.IdInsumo, c.Cantidad, i.Nombre, i.UnidadMedida, c.UnidadConsumo
                FROM ConsumoPorPedido c
                INNER JOIN Inventario i ON c.IdInsumo = i.IdInsumo";
            using var cmd = new SqliteCommand(query, conexion);
            using var reader = cmd.ExecuteReader();

            while (reader.Read())
            {
                lista.Add(new ConsumoLavado
                {
                    IdConsumo = reader.GetInt32(0),
                    IdInsumo = reader.GetInt32(1),
                    Cantidad = reader.GetDouble(2),
                    NombreInsumo = reader.GetString(3),
                    UnidadMedida = reader.GetString(4),
                    UnidadConsumo = reader.IsDBNull(5) ? "" : reader.GetString(5)
                });
            }
            return lista;
        }

        public void Guardar(ConsumoLavado consumo)
        {
            using var conexion = new SqliteConnection(Config.ConnectionString);
            conexion.Open();

            if (consumo.IdConsumo > 0)
            {
                string query = "UPDATE ConsumoPorPedido SET IdInsumo = @IdInsumo, Cantidad = @Cantidad, UnidadConsumo = @UnidadConsumo WHERE IdConsumo = @IdConsumo";
                using var cmd = new SqliteCommand(query, conexion);
                cmd.Parameters.AddWithValue("@IdInsumo", consumo.IdInsumo);
                cmd.Parameters.AddWithValue("@Cantidad", consumo.Cantidad);
                cmd.Parameters.AddWithValue("@UnidadConsumo", consumo.UnidadConsumo ?? "");
                cmd.Parameters.AddWithValue("@IdConsumo", consumo.IdConsumo);
                cmd.ExecuteNonQuery();
            }
            else
            {
                string query = "INSERT INTO ConsumoPorPedido (IdInsumo, Cantidad, UnidadConsumo) VALUES (@IdInsumo, @Cantidad, @UnidadConsumo)";
                using var cmd = new SqliteCommand(query, conexion);
                cmd.Parameters.AddWithValue("@IdInsumo", consumo.IdInsumo);
                cmd.Parameters.AddWithValue("@Cantidad", consumo.Cantidad);
                cmd.Parameters.AddWithValue("@UnidadConsumo", consumo.UnidadConsumo ?? "");
                cmd.ExecuteNonQuery();
            }
        }

        public void Eliminar(int idConsumo)
        {
            using var conexion = new SqliteConnection(Config.ConnectionString);
            conexion.Open();

            string query = "DELETE FROM ConsumoPorPedido WHERE IdConsumo = @IdConsumo";
            using var cmd = new SqliteCommand(query, conexion);
            cmd.Parameters.AddWithValue("@IdConsumo", idConsumo);
            cmd.ExecuteNonQuery();
        }
    }
}
