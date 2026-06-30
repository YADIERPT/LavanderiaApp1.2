using LavanderiaApp.Modelos;
using LavanderiaApp.Repositorios;
using System.Collections.Generic;

namespace LavanderiaApp;

public class PedidoServicio
{
    private PedidoRepositorio _pedidoRepo;
    private DetallePedidoRepositorio _detalleRepo;

    public PedidoServicio()
    {
        _pedidoRepo = new PedidoRepositorio();
        _detalleRepo = new DetallePedidoRepositorio();
    }

    public string RegistrarPedido(Pedido pedido, List<DetallePedido> detalles)
    {
        if (pedido.Total <= 0)
        {
              return "El total del Pedido debe ser mayor a 0.";
        }

        if (detalles == null || detalles.Count == 0)
        {
            return "El pedido debe tener al menos un servicio.";
        }

        try
        {
            int idPedido = _pedidoRepo.Guardar(pedido);
            
            foreach (var detalle in detalles)
            {
                detalle.IdPedido = idPedido;
                _detalleRepo.Guardar(detalle);
            }
            
            return "Éxito";
        }
        catch (System.Exception ex)
        {
            return $"Error al guardar en la base de datos: {ex.Message}";
        }
    }

    public string EntregarPedido(int idPedido)
    {
        try
        {
            _pedidoRepo.Entregar(idPedido);
            return "Éxito";
        }
        catch (System.Exception ex)
        {
            return $"Error al entregar pedido: {ex.Message}";
        }
    }

    public string ActualizarEstado(int idPedido, string nuevoEstado)
    {
        try
        {
            _pedidoRepo.ActualizarEstado(idPedido, nuevoEstado);
            return "Éxito";
        }
        catch (System.Exception ex)
        {
            return $"Error al actualizar estado: {ex.Message}";
        }
    }
}
