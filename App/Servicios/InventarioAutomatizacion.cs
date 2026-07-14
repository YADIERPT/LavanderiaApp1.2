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
            using var transaccion = conexion.BeginTransaction();

            try
            {
                // 1. Verificar si el pedido ya fue procesado dentro de la transacción para evitar condiciones de carrera
                string queryCheck = "SELECT InventarioRestado FROM Pedidos WHERE IdPedido = @IdPedido";
                using (var cmdCheck = new SqliteCommand(queryCheck, conexion, transaccion))
                {
                    cmdCheck.Parameters.AddWithValue("@IdPedido", idPedido);
                    var result = cmdCheck.ExecuteScalar();
                    if (result != null && Convert.ToInt32(result) == 1)
                    {
                        transaccion.Rollback();
                        return;
                    }
                }

                // 2. Obtener los detalles del pedido (servicios y cantidades)
                var detalles = new List<(int IdServicio, double Cantidad)>();
                string queryDetalles = "SELECT IdServicio, Cantidad FROM DetallesPedido WHERE IdPedido = @IdPedido";
                using (var cmdDetalles = new SqliteCommand(queryDetalles, conexion, transaccion))
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

                    foreach (var rec in recetaInsumos)
                    {
                        double cantidadDeduccion = det.Cantidad * rec.CantidadReceta;

                        // 4. Obtener costo actual del insumo
                        decimal costoUnitario = 0.0m;
                        string queryCosto = "SELECT CostoUnitario FROM Inventario WHERE IdInsumo = @IdInsumo";
                        using (var cmdCosto = new SqliteCommand(queryCosto, conexion, transaccion))
                        {
                            cmdCosto.Parameters.AddWithValue("@IdInsumo", rec.IdInsumo);
                            var objCosto = cmdCosto.ExecuteScalar();
                            if (objCosto != null && objCosto != DBNull.Value)
                            {
                                costoUnitario = Convert.ToDecimal(objCosto);
                            }
                        }

                        costoTotalInsumos += costoUnitario * (decimal)cantidadDeduccion;

                        // 5. Restar del inventario
                        string queryUpdateInv = "UPDATE Inventario SET Cantidad = Cantidad - @Deduccion WHERE IdInsumo = @IdInsumo";
                        using (var cmdUpdateInv = new SqliteCommand(queryUpdateInv, conexion, transaccion))
                        {
                            cmdUpdateInv.Parameters.AddWithValue("@Deduccion", cantidadDeduccion);
                            cmdUpdateInv.Parameters.AddWithValue("@IdInsumo", rec.IdInsumo);
                            cmdUpdateInv.ExecuteNonQuery();
                        }

                        // 6. Registrar en historial de insumos
                        string queryHistorial = @"
                            INSERT INTO HistorialInsumos (IdInsumo, CantidadAnterior, CantidadNueva, Motivo, UsuarioResponsable, Fecha)
                            VALUES (@IdInsumo, 0, -@Cantidad, 'Consumo automático Pedido #' || @IdPedido, 'Sistema', @Fecha)";
                        using (var cmdHist = new SqliteCommand(queryHistorial, conexion, transaccion))
                        {
                            cmdHist.Parameters.AddWithValue("@IdInsumo", rec.IdInsumo);
                            cmdHist.Parameters.AddWithValue("@Cantidad", cantidadDeduccion);
                            cmdHist.Parameters.AddWithValue("@IdPedido", idPedido);
                            cmdHist.Parameters.AddWithValue("@Fecha", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));
                            cmdHist.ExecuteNonQuery();
                        }
                    }
                }

                // 7. Marcar el pedido como procesado en inventario y guardar costo
                string queryUpdatePed = "UPDATE Pedidos SET InventarioRestado = 1, CostoInsumos = @Costo WHERE IdPedido = @IdPedido";
                using (var cmdUpdatePed = new SqliteCommand(queryUpdatePed, conexion, transaccion))
                {
                    cmdUpdatePed.Parameters.AddWithValue("@Costo", costoTotalInsumos);
                    cmdUpdatePed.Parameters.AddWithValue("@IdPedido", idPedido);
                    cmdUpdatePed.ExecuteNonQuery();
                }

                transaccion.Commit();
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
