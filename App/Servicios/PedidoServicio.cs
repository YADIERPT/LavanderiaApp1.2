using LavanderiaApp.Modelos;

namespace LavanderiaApp;

public class PedidoServicio
{
    private PedidoRepositorio _pedidoRepo;

    public PedidoServicio()
    {
        _pedidoRepo = new PedidoRepositorio();
    }

    public string RegistrarPedido(Pedido pedido)
    {
        if (pedido.Total <= 0)
        {
              return "El total del Pedido debe ser mayor a 0.";
        }

        try
        {
            _pedidoRepo.Guardar(pedido);
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
