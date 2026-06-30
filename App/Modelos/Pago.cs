namespace LavanderiaApp.Modelos;

public class Pago
{
    public int IdPago { get; set; }
    public int IdPedido { get; set; }
    public MetodoPago Metodo { get; set; } 
    public DateTime FechaPago { get; set; }
    public decimal MontoPago { get; set; }
    
    public enum MetodoPago {
        Efectivo,
        Tarjeta,
        Transferencia
    }
}
