using LavanderiaApp.Modelos;
using System.Collections.Generic;

namespace LavanderiaApp.Repositorios;

public interface IDetallePedidoRepositorio
{
    void Guardar(DetallePedido detalle);
    List<DetallePedido> ObtenerPorPedido(int idPedido);
}
