namespace LavanderiaApp.Modelos;

/// <summary>
/// Representa una línea o ítem individual dentro de un pedido de lavandería,
/// detallando el servicio contratado, cantidad, precio y subtotal.
/// </summary>
public class DetallePedido
{
    public int IdDetallePedido { get; set; }
    public int IdPedido { get; set; }
    public int IdServicio { get; set; }
    public double Cantidad { get; set; }
    public decimal PrecioUnitario { get; set; }
    public decimal Subtotal { get; set; }
    public string Indicaciones { get; set; } = string.Empty;

    // Propiedad de navegación
    public Servicio? Servicio { get; set; }

    public DetallePedido() { }

    public DetallePedido(int idServicio, double cantidad, decimal precioUnitario, string indicaciones = "")
    {
        IdServicio = idServicio;
        Cantidad = cantidad;
        PrecioUnitario = precioUnitario;
        Indicaciones = indicaciones;
        RecalcularSubtotal();
    }

    /// <summary>
    /// Recalcula el subtotal en base a la cantidad y precio unitario actual.
    /// </summary>
    public decimal RecalcularSubtotal()
    {
        Subtotal = (decimal)Cantidad * PrecioUnitario;
        return Subtotal;
    }

    /// <summary>
    /// Modifica la cantidad contratada y actualiza automáticamente el subtotal.
    /// </summary>
    public void ActualizarCantidad(double nuevaCantidad)
    {
        if (nuevaCantidad > 0)
        {
            Cantidad = nuevaCantidad;
            RecalcularSubtotal();
        }
    }
}