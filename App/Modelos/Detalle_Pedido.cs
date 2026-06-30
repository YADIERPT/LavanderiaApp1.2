namespace LavanderiaApp.Modelos;

public class DetallePedido
{
    public int IdDetallePedido { get; set; }
    public int IdPedido { get; set; }
    public int IdServicio { get; set; }
    public double Cantidad { get; set; }
    public decimal PrecioUnitario { get; set; }
    public decimal Subtotal { get; set; }
}