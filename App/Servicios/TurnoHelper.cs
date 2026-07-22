using System;
using System.Collections.Generic;
using System.Linq;
using LavanderiaApp.Modelos;
using LavanderiaApp.Repositorios;

namespace LavanderiaApp.Servicios;

public static class TurnoHelper
{
    public static int ObtenerNumeroDisplay(int idPedido)
    {
        bool isAdmin = SessionManager.UsuarioActual?.EsAdmin ?? false;
        if (isAdmin)
        {
            return idPedido;
        }

        try
        {
            var pedidoRepo = new PedidoRepositorio();
            var corteRepo = new CorteCajaRepositorio();
            var pedido = pedidoRepo.ObtenerPorId(idPedido);
            if (pedido == null) return idPedido;

            var cortes = corteRepo.ObtenerTodos();
            var ultimoCorte = cortes.OrderByDescending(c => c.Fecha).FirstOrDefault();

            var pedidosHoy = pedidoRepo.ObtenerTodos()
                .Where(p => p.FechaRecepcion.Date == DateTime.Today)
                .OrderBy(p => p.FechaRecepcion)
                .ThenBy(p => p.IdPedido)
                .ToList();

            if (ultimoCorte != null && ultimoCorte.Fecha.Date == DateTime.Today)
            {
                pedidosHoy = pedidosHoy.Where(p => p.FechaRecepcion > ultimoCorte.Fecha).ToList();
            }

            int idx = pedidosHoy.FindIndex(p => p.IdPedido == idPedido);
            if (idx >= 0)
            {
                return idx + 1;
            }
        }
        catch
        {
            // Si ocurre algún error, retornamos el id original como respaldo
        }

        return idPedido;
    }
}
