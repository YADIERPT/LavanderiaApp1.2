using System;
using System.Collections.Generic;
using Microsoft.Data.Sqlite;

namespace LavanderiaApp.Servicios;

public static class InventarioAutomatizacion
{
    /// <summary>
    /// Procesa de forma automática la deducción de insumos en el inventario
    /// basándose en la "receta" configurada para cada servicio del pedido.
    /// También calcula el costo total real de los insumos y lo guarda en el pedido.
    /// </summary>
    /// <param name="idPedido">ID del pedido a procesar.</param>
    public static void ProcesarConsumoInventario(int idPedido)
    {
        try
        {
            using var conexion = new SqliteConnection(Config.ConnectionString);
            conexion.Open();

            // 1. Verificar si el pedido ya fue procesado para evitar doble deducción
            string queryCheck = "SELECT InventarioRestado FROM Pedidos WHERE IdPedido = @IdPedido";
            using (var cmdCheck = new SqliteCommand(queryCheck, conexion))
            {
                cmdCheck.Parameters.AddWithValue("@IdPedido", idPedido);
                var result = cmdCheck.ExecuteScalar();
                if (result != null && Convert.ToInt32(result) == 1)
                {
                    // Ya se restó el inventario para este pedido
                    return;
                }
            }

            // 2. Obtener los detalles del pedido (servicios y cantidades)
            var detalles = new List<(int IdServicio, double Cantidad)>();
            string queryDetalles = "SELECT IdServicio, Cantidad FROM DetallesPedido WHERE IdPedido = @IdPedido";
            using (var cmdDetalles = new SqliteCommand(queryDetalles, conexion))
            {
                cmdDetalles.Parameters.AddWithValue("@IdPedido", idPedido);
                using var reader = cmdDetalles.ExecuteReader();
                while (reader.Read())
                {
                    detalles.Add((
                        reader.GetInt32(0),
                        reader.GetDouble(1)
                    ));
                }
            }

            decimal costoTotalInsumos = 0.0m;
            using var transaccion = conexion.BeginTransaction();

            try
            {
                foreach (var det in detalles)
                {
                    // 3. Obtener la receta para este servicio
                    var recetaInsumos = new List<(int IdInsumo, double CantidadReceta)>();
                    string queryReceta = "SELECT IdInsumo, Cantidad FROM Recetas WHERE IdServicio = @IdServicio";
                    using (var cmdReceta = new SqliteCommand(queryReceta, conexion, transaccion))
                    {
                        cmdReceta.Parameters.AddWithValue("@IdServicio", det.IdServicio);
                        using var reader = cmdReceta.ExecuteReader();
                        while (reader.Read())
                        {
                            recetaInsumos.Add((
                                reader.GetInt32(0),
                                reader.GetDouble(1)
                            ));
                        }
                    }

                    // 4. Por cada insumo de la receta, restar y calcular costo
                    foreach (var rec in recetaInsumos)
                    {
                        double cantidadConsumida = rec.CantidadReceta * det.Cantidad;

                        // Obtener precio unitario del insumo para costear
                        decimal precioUnitario = 0.0m;
                        double cantidadActual = 0.0;
                        string queryInsumo = "SELECT Cantidad, PrecioUnitario FROM Inventario WHERE IdInsumo = @IdInsumo";
                        using (var cmdInsumo = new SqliteCommand(queryInsumo, conexion, transaccion))
                        {
                            cmdInsumo.Parameters.AddWithValue("@IdInsumo", rec.IdInsumo);
                            using var reader = cmdInsumo.ExecuteReader();
                            if (reader.Read())
                            {
                                cantidadActual = reader.GetDouble(0);
                                precioUnitario = reader.GetDecimal(1);
                            }
                        }

                        // Calcular costo consumido
                        decimal costoConsumido = (decimal)cantidadConsumida * precioUnitario;
                        costoTotalInsumos += costoConsumido;

                        // Restar del inventario
                        double nuevaCantidad = Math.Max(0.0, cantidadActual - cantidadConsumida);
                        string queryUpdateStock = @"
                            UPDATE Inventario 
                            SET Cantidad = @Cantidad, UltimaActualizacion = @Fecha 
                            WHERE IdInsumo = @IdInsumo";
                        using (var cmdUpdate = new SqliteCommand(queryUpdateStock, conexion, transaccion))
                        {
                            cmdUpdate.Parameters.AddWithValue("@Cantidad", nuevaCantidad);
                            cmdUpdate.Parameters.AddWithValue("@Fecha", DateTime.Now.ToString("yyyy-MM-dd"));
                            cmdUpdate.Parameters.AddWithValue("@IdInsumo", rec.IdInsumo);
                            cmdUpdate.ExecuteNonQuery();
                        }
                    }
                }

                // 5. Actualizar el pedido para marcarlo como restado y asignarle el costo de insumos
                string queryUpdatePedido = @"
                    UPDATE Pedidos 
                    SET InventarioRestado = 1, CostoInsumos = @Costo 
                    WHERE IdPedido = @IdPedido";
                using (var cmdUpdatePed = new SqliteCommand(queryUpdatePedido, conexion, transaccion))
                {
                    cmdUpdatePed.Parameters.AddWithValue("@Costo", costoTotalInsumos);
                    cmdUpdatePed.Parameters.AddWithValue("@IdPedido", idPedido);
                    cmdUpdatePed.ExecuteNonQuery();
                }

                transaccion.Commit();
                
                // Mostrar alerta local en consola/depuración
                System.Diagnostics.Debug.WriteLine($"Inventario automatizado procesado para Pedido #{idPedido}. Costo de insumos: {costoTotalInsumos:C}");
            }
            catch (Exception ex)
            {
                transaccion.Rollback();
                System.Diagnostics.Debug.WriteLine($"Error al restar inventario del pedido #{idPedido}: {ex.Message}");
                throw;
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error general en automatización de inventario: {ex.Message}");
        }
    }
}
