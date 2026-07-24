using LavanderiaApp.Modelos;
using LavanderiaApp.Repositorios;
using System;

namespace LavanderiaApp.Servicios;

public class PagoServicio
{
    private readonly IPagoRepositorio _pagoRepo;
    private readonly IPedidoRepositorio _pedidoRepo;

    public PagoServicio(IPagoRepositorio pagoRepo, IPedidoRepositorio pedidoRepo)
    {
        _pagoRepo = pagoRepo;
        _pedidoRepo = pedidoRepo;
    }

    public string ProcesarPago(int idPedido, decimal monto, Pago.MetodoPago metodo)
    {
        try
        {
            var pago = new Pago
            {
                IdPedido = idPedido,
                MontoPago = monto,
                Metodo = metodo,
                FechaPago = DateTime.Now
            };

            _pagoRepo.Guardar(pago);
            _pedidoRepo.Entregar(idPedido); // Cambia estado a Entregado y pone fecha entrega

            return "Éxito";
        }
        catch (Exception ex)
        {
            return $"Error al procesar el pago: {ex.Message}";
        }
    }
}