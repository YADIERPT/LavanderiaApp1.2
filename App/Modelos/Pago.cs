namespace LavanderiaApp.Modelos;

/// <summary>
/// Representa la transacción financiera de pago vinculada a un pedido de lavandería.
/// </summary>
public class Pago
{
    public int IdPago { get; set; }
    public int IdPedido { get; set; }
    public MetodoPago Metodo { get; set; } = MetodoPago.Efectivo;
    public DateTime FechaPago { get; set; } = DateTime.Now;
    public decimal MontoPago { get; set; }
    public decimal MontoRecibido { get; set; }
    public string Referencia { get; set; } = string.Empty;
    public string Notas { get; set; } = string.Empty;
    
    public enum MetodoPago {
        Efectivo,
        Tarjeta,
        Transferencia
    }

    public Pago() { }

    public Pago(int idPedido, MetodoPago metodo, decimal montoPago, decimal montoRecibido = 0m, string referencia = "")
    {
        IdPedido = idPedido;
        Metodo = metodo;
        MontoPago = montoPago;
        MontoRecibido = montoRecibido > 0 ? montoRecibido : montoPago;
        Referencia = referencia;
        FechaPago = DateTime.Now;
    }

    /// <summary>
    /// Calcula el cambio a devolver al cliente cuando paga en efectivo.
    /// </summary>
    public decimal Cambio => MontoRecibido > MontoPago ? MontoRecibido - MontoPago : 0m;

    /// <summary>
    /// Devuelve el método de pago como cadena legible.
    /// </summary>
    public string MetodoTexto => Metodo.ToString();

    /// <summary>
    /// Valida si el pago es correcto antes de procesarlo.
    /// </summary>
    public bool ValidarPago(out List<string> errores)
    {
        errores = new List<string>();

        if (IdPedido <= 0)
            errores.Add("El ID de pedido asociado debe ser válido.");

        if (MontoPago <= 0)
            errores.Add("El monto del pago debe ser mayor a cero.");

        if (Metodo == MetodoPago.Efectivo && MontoRecibido < MontoPago)
            errores.Add("El monto recibido no cubre el importe a pagar.");

        return errores.Count == 0;
    }
}
