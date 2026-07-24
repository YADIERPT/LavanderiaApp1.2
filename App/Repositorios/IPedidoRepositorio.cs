using LavanderiaApp.Modelos;
using System.Collections.Generic;

namespace LavanderiaApp.Repositorios;

public interface IPedidoRepositorio
{
    int Guardar(Pedido pedido);
    List<Pedido> ObtenerTodos();
    Pedido ObtenerPorId(int idPedido);
    void Entregar(int idPedido);
    void ActualizarEstado(int idPedido, string nuevoEstado);
    void ActualizarEstadoYMaquina(int idPedido, string nuevoEstado, string maquina);
    void ActualizarInventarioRestado(int idPedido, int restado, decimal costo);
    void ActualizarPedido(Pedido pedido);
}
