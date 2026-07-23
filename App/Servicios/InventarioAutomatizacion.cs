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
        if (!BusinessConfig.Current.InventarioActivo) return;

        try
        {
            using var conexion = new SqliteConnection(Config.ConnectionString);
            conexion.Open();
            using var transaccion = conexion.BeginTransaction();

            try
            {
                // 1. Verificar si el pedido ya fue procesado
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

                // Calcular el total de Kg (o cantidad) del pedido
                double totalKgPedido = 0;
                string queryKg = @"
                    SELECT SUM(dp.Cantidad) 
                    FROM DetallesPedido dp 
                    WHERE dp.IdPedido = @IdPedido";
                using (var cmdKg = new SqliteCommand(queryKg, conexion, transaccion))
                {
                    cmdKg.Parameters.AddWithValue("@IdPedido", idPedido);
                    var resKg = cmdKg.ExecuteScalar();
                    if (resKg != null && resKg != DBNull.Value)
                    {
                        totalKgPedido = Convert.ToDouble(resKg);
                    }
                }
                
                // Si por alguna razón es 0, al menos contar como 1 para que consuma algo
                if (totalKgPedido <= 0) totalKgPedido = 1;


                // 2. Obtener lista de insumos a consumir por cada lavado (configuración global)
                var consumos = new List<(int IdInsumo, double Cantidad, string UnidadConsumo)>();
                string queryConsumo = "SELECT IdInsumo, Cantidad, UnidadConsumo FROM ConsumoPorPedido";
                using (var cmdConsumo = new SqliteCommand(queryConsumo, conexion, transaccion))
                {
                    using var reader = cmdConsumo.ExecuteReader();
                    while (reader.Read())
                    {
                        consumos.Add((reader.GetInt32(0), reader.GetDouble(1), reader.IsDBNull(2) ? "" : reader.GetString(2)));
                    }
                }

                decimal costoTotalInsumos = 0.0m;

                foreach (var con in consumos)
                {
                    // Obtener datos actuales del insumo
                    double cantidadActual = 0;
                    decimal precioUnitario = 0;
                    double capacidadEnvase = 0;
                    string nombreInsumo = "";
                    string unidadInsumo = "";
                    
                    string queryInsumo = "SELECT Cantidad, PrecioUnitario, CapacidadEnvase, Nombre, UnidadMedida FROM Inventario WHERE IdInsumo = @IdInsumo";
                    using (var cmdInsumo = new SqliteCommand(queryInsumo, conexion, transaccion))
                    {
                        cmdInsumo.Parameters.AddWithValue("@IdInsumo", con.IdInsumo);
                        using var reader = cmdInsumo.ExecuteReader();
                        if (reader.Read())
                        {
                            cantidadActual = reader.GetDouble(0);
                            precioUnitario = reader.GetDecimal(1);
                            capacidadEnvase = reader.GetDouble(2);
                            nombreInsumo = reader.GetString(3);
                            unidadInsumo = reader.GetString(4);
                        }
                    }

                    // Sumamos el costo de este insumo al pedido (proporcional o unitario? normalmente es un costo estimado, lo tomaremos como 0 si no se maneja, o una fracción del bote)
                    // El costo real se registra cuando el bote se vacía.
                    

                    // Conversión de unidades
                    double factorConversion = 1.0;
                    string uInv = unidadInsumo.ToLower();
                    string uCon = con.UnidadConsumo.ToLower();
                    
                    if (uInv == "kg" && uCon == "g") factorConversion = 0.001;
                    else if (uInv == "kg" && uCon == "mg") factorConversion = 0.000001;
                    else if (uInv == "litros" && uCon == "ml") factorConversion = 0.001;
                    else if (uInv == "galones" && uCon == "ml") factorConversion = 1.0 / 3785.41;
                    else if (uInv == "g" && uCon == "mg") factorConversion = 0.001;
                    else if (uInv == "g" && uCon == "kg") factorConversion = 1000.0;
                    else if (uInv == "ml" && uCon == "litros") factorConversion = 1000.0;
                    
                    double cantidadDeduccion = (con.Cantidad * totalKgPedido) * factorConversion;
                    double nuevaCantidad = cantidadActual - cantidadDeduccion;

                    // Sumar al costo total de insumos del pedido (costo proporcional de lo que se acaba de gastar)
                    if (capacidadEnvase > 0)
                    {
                        costoTotalInsumos += (decimal)(cantidadDeduccion / capacidadEnvase) * precioUnitario;
                    }


                    // Actualizar Inventario
                    string queryUpdateInv = "UPDATE Inventario SET Cantidad = @NuevaCantidad WHERE IdInsumo = @IdInsumo";
                    using (var cmdUpdateInv = new SqliteCommand(queryUpdateInv, conexion, transaccion))
                    {
                        cmdUpdateInv.Parameters.AddWithValue("@NuevaCantidad", nuevaCantidad);
                        cmdUpdateInv.Parameters.AddWithValue("@IdInsumo", con.IdInsumo);
                        cmdUpdateInv.ExecuteNonQuery();
                    }

                    // Registrar en historial de insumos
                    string queryHistorial = @"
                        INSERT INTO HistorialInsumos (IdInsumo, CantidadAnterior, CantidadNueva, Motivo, UsuarioResponsable, Fecha)
                        VALUES (@IdInsumo, @CantAnt, @CantNueva, 'Consumo automático Pedido #' || @IdPedido, 'Sistema', @Fecha)";
                    using (var cmdHist = new SqliteCommand(queryHistorial, conexion, transaccion))
                    {
                        cmdHist.Parameters.AddWithValue("@IdInsumo", con.IdInsumo);
                        cmdHist.Parameters.AddWithValue("@CantAnt", cantidadActual);
                        cmdHist.Parameters.AddWithValue("@CantNueva", nuevaCantidad);
                        cmdHist.Parameters.AddWithValue("@IdPedido", idPedido);
                        cmdHist.Parameters.AddWithValue("@Fecha", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));
                        cmdHist.ExecuteNonQuery();
                    }
                }

                // Marcar el pedido como procesado y guardar su costo de insumos real
                string queryUpdatePed = "UPDATE Pedidos SET InventarioRestado = 1, CostoInsumos = @CostoInsumos WHERE IdPedido = @IdPedido";
                using (var cmdUpdatePed = new SqliteCommand(queryUpdatePed, conexion, transaccion))
                {
                    cmdUpdatePed.Parameters.AddWithValue("@IdPedido", idPedido);
                    cmdUpdatePed.Parameters.AddWithValue("@CostoInsumos", costoTotalInsumos);
                    cmdUpdatePed.ExecuteNonQuery();
                }

                transaccion.Commit();
            }
            catch (Exception)
            {
                transaccion.Rollback();
                throw;
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error general en automatización de inventario: {ex.Message}");
        }
    }
}
