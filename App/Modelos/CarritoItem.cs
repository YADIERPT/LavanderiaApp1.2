namespace LavanderiaApp.Modelos;

/// <summary>
/// Representa un elemento temporal en el carrito de compras antes de confirmar la recepción de un pedido.
/// </summary>
public class CarritoItem
{
    public int IdServicio { get; set; }
    public string NombreServicio { get; set; } = string.Empty;
    public double Cantidad { get; set; } = 1.0;
    public decimal PrecioUnitario { get; set; }
    public decimal Subtotal { get; set; }
    public string UnidadMedida { get; set; } = "Kg";
    public string Indicaciones { get; set; } = string.Empty;

    public CarritoItem() { }

    public CarritoItem(Servicio servicio, double cantidad = 1.0, string indicaciones = "")
    {
        if (servicio != null)
        {
            IdServicio = servicio.IdServicio;
            NombreServicio = servicio.Nombre;
            PrecioUnitario = servicio.Precio;
            UnidadMedida = servicio.UnidadMedida;
        }
        Cantidad = cantidad;
        Indicaciones = indicaciones;
        RecalcularSubtotal();
    }

    /// <summary>
    /// Recalcula el subtotal en base a la cantidad y precio unitario.
    /// </summary>
    public decimal RecalcularSubtotal()
    {
        Subtotal = (decimal)Cantidad * PrecioUnitario;
        return Subtotal;
    }

    /// <summary>
    /// Incrementa la cantidad en el carrito.
    /// </summary>
    public void IncrementarCantidad(double delta = 1.0)
    {
        Cantidad += delta;
        RecalcularSubtotal();
    }

    /// <summary>
    /// Decrementa la cantidad en el carrito sin permitir valores menores o iguales a cero.
    /// </summary>
    public void DecrementarCantidad(double delta = 1.0)
    {
        if (Cantidad - delta > 0)
        {
            Cantidad -= delta;
            RecalcularSubtotal();
        }
    }

    /// <summary>
    /// Convierte la instancia del carrito a un objeto persistible de DetallePedido.
    /// </summary>
    public DetallePedido ADetallePedido(int idPedido = 0)
    {
        return new DetallePedido
        {
            IdPedido = idPedido,
            IdServicio = IdServicio,
            Cantidad = Cantidad,
            PrecioUnitario = PrecioUnitario,
            Subtotal = Subtotal,
            Indicaciones = Indicaciones
        };
    }
}
