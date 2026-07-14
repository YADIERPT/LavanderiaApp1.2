namespace LavanderiaApp.Modelos;

/// <summary>
/// Representa una orden de servicio o pedido en la lavandería, consolidando
/// cliente, recepcionista, fecha de entrada/salida, estado operativo, ítems y pagos.
/// </summary>
public class Pedido
{
    public int IdPedido { get; set; }
    public int IdCliente { get; set; }
    public int IdUsuario { get; set; }
    public DateTime FechaRecepcion { get; set; } = DateTime.Now;
    public DateTime? FechaEntrega { get; set; }
    public string Estado { get; set; } = "En espera"; // "En espera", "En proceso", "Listo", "Entregado", "Cancelado"
    public decimal Total { get; set; }
    public int InventarioRestado { get; set; } = 0;
    public decimal CostoInsumos { get; set; } = 0.0m;
    public string MaquinaAsignada { get; set; } = "";

    // Propiedades de navegación y relaciones
    public Cliente? Cliente { get; set; }
    public Usuario? Usuario { get; set; }
    public List<DetallePedido> Detalles { get; set; } = new();
    public List<Pago> Pagos { get; set; } = new();

    public Pedido() { }

    public Pedido(int idCliente, int idUsuario)
    {
        IdCliente = idCliente;
        IdUsuario = idUsuario;
        FechaRecepcion = DateTime.Now;
        Estado = "En espera";
    }

    /// <summary>
    /// Recalcula el total general del pedido a partir de la suma de los subtotales de cada detalle.
    /// </summary>
    public decimal CalcularTotal()
    {
        Total = Detalles?.Sum(d => d.Subtotal) ?? 0m;
        return Total;
    }

    /// <summary>
    /// Calcula la suma total abonada o pagada hasta la fecha.
    /// </summary>
    public decimal MontoPagado => Pagos?.Sum(p => p.MontoPago) ?? 0m;

    /// <summary>
    /// Devuelve el saldo pendiente de liquidar en el pedido.
    /// </summary>
    public decimal SaldoPendiente
    {
        get
        {
            decimal saldo = Total - MontoPagado;
            return saldo > 0 ? saldo : 0m;
        }
    }

    /// <summary>
    /// Indica si el pedido ha sido totalmente cubierto económicamente.
    /// </summary>
    public bool EstaPagado => Total > 0 && SaldoPendiente <= 0;

    /// <summary>
    /// Calcula el tiempo transcurrido desde la recepción del pedido.
    /// </summary>
    public TimeSpan TiempoTranscurrido => DateTime.Now - FechaRecepcion;

    // Estados rápidos coherentes en toda la aplicación
    public bool EsEnEspera => string.Equals(Estado, "En espera", StringComparison.OrdinalIgnoreCase) ||
                              string.Equals(Estado, "En espera de lavado", StringComparison.OrdinalIgnoreCase);
    public bool EsEnLavado => string.Equals(Estado, "En Lavado", StringComparison.OrdinalIgnoreCase) ||
                              string.Equals(Estado, "Lavando", StringComparison.OrdinalIgnoreCase);
    public bool EsEnSecado => string.Equals(Estado, "En Secado", StringComparison.OrdinalIgnoreCase) ||
                              string.Equals(Estado, "Secando", StringComparison.OrdinalIgnoreCase);
    public bool EsEnProceso => EsEnLavado || EsEnSecado || string.Equals(Estado, "En proceso", StringComparison.OrdinalIgnoreCase);
    public bool EsListo => string.Equals(Estado, "Listo", StringComparison.OrdinalIgnoreCase) ||
                           string.Equals(Estado, "Listo para entregar", StringComparison.OrdinalIgnoreCase);
    public bool EsEntregado => string.Equals(Estado, "Entregado", StringComparison.OrdinalIgnoreCase);
    public bool EsCancelado => string.Equals(Estado, "Cancelado", StringComparison.OrdinalIgnoreCase);

    /// <summary>
    /// Añade un ítem/servicio al pedido y recalcula el importe total.
    /// </summary>
    public void AgregarDetalle(DetallePedido detalle)
    {
        if (detalle == null) return;
        detalle.IdPedido = IdPedido;
        Detalles.Add(detalle);
        CalcularTotal();
    }

    /// <summary>
    /// Elimina un detalle por el ID de servicio correspondiente.
    /// </summary>
    public void RemoverDetalle(int idServicio)
    {
        Detalles.RemoveAll(d => d.IdServicio == idServicio);
        CalcularTotal();
    }

    /// <summary>
    /// Registra un abono o pago completo sobre la orden.
    /// </summary>
    public void RegistrarPago(Pago pago)
    {
        if (pago == null) return;
        pago.IdPedido = IdPedido;
        Pagos.Add(pago);
    }

    /// <summary>
    /// Actualiza el estado operativo de la orden. Si se marca como Entregado, fija FechaEntrega.
    /// </summary>
    public void CambiarEstado(string nuevoEstado)
    {
        if (string.IsNullOrWhiteSpace(nuevoEstado)) return;

        Estado = nuevoEstado.Trim();
        if (EsEntregado && !FechaEntrega.HasValue)
        {
            FechaEntrega = DateTime.Now;
        }
    }

    /// <summary>
    /// Valida si el pedido cumple las reglas previas a la creación.
    /// </summary>
    public bool ValidarPedido(out List<string> errores)
    {
        errores = new List<string>();

        if (IdCliente <= 0)
            errores.Add("Se debe asignar un cliente al pedido.");

        if (IdUsuario <= 0)
            errores.Add("Se debe especificar el usuario recepcionista.");

        if (Detalles == null || Detalles.Count == 0)
            errores.Add("El pedido debe contener al menos un servicio.");

        return errores.Count == 0;
    }
}