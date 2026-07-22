using System;
using Microsoft.Data.Sqlite;
using LavanderiaApp.Repositorios;
using LavanderiaApp.Modelos;

namespace LavanderiaApp.Servicios;

public static class MaquinasAutomatizacion
{
    /// <summary>
    /// Incrementa de manera automática los ciclos operados de la máquina correspondiente
    /// al procesar o cambiar de estado un pedido. Si alcanza o está cerca del límite
    /// de mantenimiento preventivo, cambia el estado automáticamente a ALERT y emite alertas.
    /// </summary>
    public static void RegistrarCicloYValidarMantenimiento(int idPedido, string nuevoEstado)
    {
        if (string.IsNullOrWhiteSpace(nuevoEstado)) return;

        // Solo procesamos cuando pasa al estado de Listo para entregar o PENDIENTE PARA ENTREGA (ciclo completado)
        bool esCicloActivo = nuevoEstado.Equals("Listo para entregar", StringComparison.OrdinalIgnoreCase) ||
                             nuevoEstado.Equals("PENDIENTE PARA ENTREGA", StringComparison.OrdinalIgnoreCase);

        if (!esCicloActivo) return;

        try
        {
            using var conexion = new SqliteConnection(Config.ConnectionString);
            conexion.Open();

            // 1. Obtener máquina asignada del pedido
            string maquinaAsignada = "";
            string queryPedido = "SELECT MaquinaAsignada FROM Pedidos WHERE IdPedido = @IdPedido";
            using (var cmdP = new SqliteCommand(queryPedido, conexion))
            {
                cmdP.Parameters.AddWithValue("@IdPedido", idPedido);
                var objM = cmdP.ExecuteScalar();
                if (objM != null && objM != DBNull.Value)
                {
                    maquinaAsignada = objM.ToString() ?? "";
                }
            }

            int idMaquina = 0;
            string nombreMaquina = "";
            int ciclosOperados = 0;
            int proxMantenimientoCiclos = 0;

            // 2. Buscar la máquina específica por nombre o por tipo de ciclo
            string queryBuscar = "";
            SqliteCommand cmdBuscar;

            if (!string.IsNullOrWhiteSpace(maquinaAsignada))
            {
                queryBuscar = "SELECT IdMaquina, Nombre, CiclosOperados, ProxMantenimientoCiclos FROM Maquinas WHERE Nombre = @Nombre LIMIT 1";
                cmdBuscar = new SqliteCommand(queryBuscar, conexion);
                cmdBuscar.Parameters.AddWithValue("@Nombre", maquinaAsignada);
            }
            else if (nuevoEstado.Equals("En Lavado", StringComparison.OrdinalIgnoreCase))
            {
                queryBuscar = "SELECT IdMaquina, Nombre, CiclosOperados, ProxMantenimientoCiclos FROM Maquinas WHERE Nombre LIKE '%Lavadora%' OR Nombre LIKE '%Lavado%' LIMIT 1";
                cmdBuscar = new SqliteCommand(queryBuscar, conexion);
            }
            else if (nuevoEstado.Equals("En Secado", StringComparison.OrdinalIgnoreCase))
            {
                queryBuscar = "SELECT IdMaquina, Nombre, CiclosOperados, ProxMantenimientoCiclos FROM Maquinas WHERE Nombre LIKE '%Secadora%' OR Nombre LIKE '%Secado%' LIMIT 1";
                cmdBuscar = new SqliteCommand(queryBuscar, conexion);
            }
            else
            {
                return;
            }

            using (cmdBuscar)
            {
                using var reader = cmdBuscar.ExecuteReader();
                if (reader.Read())
                {
                    idMaquina = reader.GetInt32(0);
                    nombreMaquina = reader.IsDBNull(1) ? "" : reader.GetString(1);
                    ciclosOperados = reader.GetInt32(2);
                    proxMantenimientoCiclos = reader.GetInt32(3);
                }
            }

            if (idMaquina <= 0) return;

            // 3. Incrementar ciclo operado en 1
            ciclosOperados++;
            string updateCiclos = "UPDATE Maquinas SET CiclosOperados = @Ciclos WHERE IdMaquina = @IdMaquina";
            using (var cmdUpd = new SqliteCommand(updateCiclos, conexion))
            {
                cmdUpd.Parameters.AddWithValue("@Ciclos", ciclosOperados);
                cmdUpd.Parameters.AddWithValue("@IdMaquina", idMaquina);
                cmdUpd.ExecuteNonQuery();
            }

            System.Diagnostics.Debug.WriteLine($"[MaquinasAutomatizacion] Máquina #{idMaquina} ({nombreMaquina}) incrementada a {ciclosOperados} ciclos.");

            // 4. Validar límite para mantenimiento preventivo automático
            if (proxMantenimientoCiclos > 0 && ciclosOperados >= proxMantenimientoCiclos)
            {
                // Poner en estado ALERT automáticamente
                string setMantenimiento = "UPDATE Maquinas SET Status = 'ALERT' WHERE IdMaquina = @IdMaquina";
                using (var cmdAlert = new SqliteCommand(setMantenimiento, conexion))
                {
                    cmdAlert.Parameters.AddWithValue("@IdMaquina", idMaquina);
                    cmdAlert.ExecuteNonQuery();
                }

                // Registrar auditoría de mantenimiento automático
                try
                {
                    new AuditoriaRepositorio().Guardar(new Auditoria
                    {
                        Fecha = DateTime.Now,
                        Usuario = "Sistema Automático",
                        Modulo = "Maquinas",
                        Accion = "Mantenimiento Preventivo",
                        Detalle = $"La máquina '{nombreMaquina}' alcanzó {ciclosOperados} ciclos (Límite configurado: {proxMantenimientoCiclos}). Se puso automáticamente en mantenimiento."
                    });
                }
                catch { }

                // Mostrar alerta visual Toast
                ToastService.Show(
                    "¡MANTENIMIENTO PREVENTIVO REQUERIDO!",
                    $"La {nombreMaquina} alcanzó el límite de {proxMantenimientoCiclos} ciclos ({ciclosOperados} operados) y se puso automáticamente en mantenimiento.",
                    "danger"
                );
            }
            else if (proxMantenimientoCiclos > 0 && proxMantenimientoCiclos - ciclosOperados <= 5)
            {
                // Alerta cercana al límite (a 5 o menos ciclos de distancia)
                int restantes = proxMantenimientoCiclos - ciclosOperados;
                ToastService.Show(
                    "Alerta de Mantenimiento Próximo",
                    $"La {nombreMaquina} tiene {ciclosOperados} ciclos operados. Está a solo {restantes} ciclo(s) del mantenimiento preventivo.",
                    "warning"
                );
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[MaquinasAutomatizacion] Error: {ex.Message}");
        }
    }

    public static bool EstaMaquinaOcupadaPorOtroPedido(string nombreMaquina, int idPedidoActual)
    {
        if (string.IsNullOrWhiteSpace(nombreMaquina)) return false;
        try
        {
            using var conexion = new SqliteConnection(Config.ConnectionString);
            conexion.Open();
            string query = @"SELECT COUNT(*) FROM Pedidos 
                             WHERE MaquinaAsignada = @Nombre 
                             AND IdPedido != @IdPedido 
                             AND Estado = 'En Lavado'";
            using var cmd = new SqliteCommand(query, conexion);
            cmd.Parameters.AddWithValue("@Nombre", nombreMaquina);
            cmd.Parameters.AddWithValue("@IdPedido", idPedidoActual);
            var count = Convert.ToInt32(cmd.ExecuteScalar());
            return count > 0;
        }
        catch
        {
            return false;
        }
    }

    public static void AsignarMaquina(string nombreMaquina)
    {
        if (string.IsNullOrWhiteSpace(nombreMaquina)) return;
        SincronizarEstadoMaquina(nombreMaquina);
    }

    public static void LiberarMaquina(string nombreMaquina)
    {
        if (string.IsNullOrWhiteSpace(nombreMaquina)) return;
        try
        {
            using var conexion = new SqliteConnection(Config.ConnectionString);
            conexion.Open();
            
            // Revisar si hay pedidos "En espera" encolados para esta máquina
            string queryQueue = "SELECT IdPedido FROM Pedidos WHERE MaquinaAsignada = @Nombre AND Estado = 'En espera' ORDER BY FechaRecepcion ASC LIMIT 1";
            int queuedId = 0;
            using (var cmdQ = new SqliteCommand(queryQueue, conexion))
            {
                cmdQ.Parameters.AddWithValue("@Nombre", nombreMaquina);
                var result = cmdQ.ExecuteScalar();
                if (result != null && result != DBNull.Value)
                {
                    queuedId = Convert.ToInt32(result);
                }
            }
            
            if (queuedId > 0)
            {
                // Hay un pedido encolado, pasarlo a "En espera de lavado"
                string updateOrder = "UPDATE Pedidos SET Estado = 'En espera de lavado' WHERE IdPedido = @IdPedido";
                using (var cmdUpdateOrder = new SqliteCommand(updateOrder, conexion))
                {
                    cmdUpdateOrder.Parameters.AddWithValue("@IdPedido", queuedId);
                    cmdUpdateOrder.ExecuteNonQuery();
                }
                System.Diagnostics.Debug.WriteLine($"[MaquinasAutomatizacion] Pedido #{queuedId} auto-asignado a máquina '{nombreMaquina}'");
            }
        }
        catch { }
        finally
        {
            SincronizarEstadoMaquina(nombreMaquina);
        }
    }

    public static void SincronizarEstadoMaquina(string nombreMaquina)
    {
        if (string.IsNullOrWhiteSpace(nombreMaquina)) return;
        try
        {
            using var conexion = new SqliteConnection(Config.ConnectionString);
            conexion.Open();
            string checkStatus = "SELECT Status FROM Maquinas WHERE Nombre = @Nombre";
            using (var cmdCheck = new SqliteCommand(checkStatus, conexion))
            {
                cmdCheck.Parameters.AddWithValue("@Nombre", nombreMaquina);
                var statusActual = cmdCheck.ExecuteScalar()?.ToString();
                if (statusActual != null && (statusActual.Equals("MANTENIMIENTO", StringComparison.OrdinalIgnoreCase) || statusActual.Equals("ALERT", StringComparison.OrdinalIgnoreCase)))
                {
                    return;
                }
            }

            string checkLavado = "SELECT COUNT(*) FROM Pedidos WHERE MaquinaAsignada = @Nombre AND Estado = 'En Lavado'";
            using var cmdCount = new SqliteCommand(checkLavado, conexion);
            cmdCount.Parameters.AddWithValue("@Nombre", nombreMaquina);
            var count = Convert.ToInt32(cmdCount.ExecuteScalar());

            string nuevoStatus = count > 0 ? "EN USO" : "ACTIVA";
            string updateStatus = "UPDATE Maquinas SET Status = @NuevoStatus WHERE Nombre = @Nombre AND Status != 'MANTENIMIENTO' AND Status != 'ALERT'";
            using var cmdUpdate = new SqliteCommand(updateStatus, conexion);
            cmdUpdate.Parameters.AddWithValue("@NuevoStatus", nuevoStatus);
            cmdUpdate.Parameters.AddWithValue("@Nombre", nombreMaquina);
            cmdUpdate.ExecuteNonQuery();
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[MaquinasAutomatizacion] Error al sincronizar máquina: {ex.Message}");
        }
    }

    public static void SincronizarTodasLasMaquinas()
    {
        try
        {
            using var conexion = new SqliteConnection(Config.ConnectionString);
            conexion.Open();
            string getMaquinas = "SELECT Nombre FROM Maquinas WHERE Status != 'MANTENIMIENTO' AND Status != 'ALERT'";
            using var cmd = new SqliteCommand(getMaquinas, conexion);
            using var reader = cmd.ExecuteReader();
            var nombres = new List<string>();
            while (reader.Read())
            {
                nombres.Add(reader.GetString(0));
            }
            reader.Close();

            foreach (var nombre in nombres)
            {
                SincronizarEstadoMaquina(nombre);
            }
        }
        catch { }
    }
}
