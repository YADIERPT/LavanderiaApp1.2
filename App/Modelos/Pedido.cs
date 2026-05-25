namespace LavanderiaApp.Modelos;

public class Pedido
{
    public int IdPedido { get; set; }
    public int IdCliente  { get; set; }
    public int IdUsuario { get; set; }
    public DateTime FechaRecepcion { get; set; }
    public DateTime? FechaEntrega { get; set; }
    public string Estado { get; set; }
    public decimal Total  { get; set; }
    
    
}