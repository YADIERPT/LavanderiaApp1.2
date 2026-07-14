using System.Text;
using LavanderiaApp.Servicios;

namespace LavanderiaApp.Modelos;

/// <summary>
/// Representa el comprobante o comprobante impreso (Ticket) de un pedido para el cliente.
/// Permite generar representaciones formateadas para impresoras térmicas o visualización.
/// </summary>
public class Ticket
{
    public int IdTicket { get; set; }
    public int IdPedido { get; set; }
    public DateTime Fecha { get; set; } = DateTime.Now;
    public string Codigo { get; set; } = string.Empty;
    public string ClienteNombre { get; set; } = string.Empty;
    public string TelefonoCliente { get; set; } = string.Empty;
    public string AtendidoPor { get; set; } = string.Empty;
    public decimal Total { get; set; }
    public decimal MontoPagado { get; set; }
    public decimal SaldoPendiente { get; set; }
    public decimal Cambio { get; set; }
    public List<CarritoItem> Items { get; set; } = new();

    public string Encabezado { get; set; } = BusinessConfig.Current?.NombreNegocio ?? "LAVANDERÍA VILLAS DEL SUR";
    public string DireccionEstablecimiento { get; set; } = BusinessConfig.Current?.Direccion ?? "Villas del Sur, Calle Principal #123";
    public string TelefonoEstablecimiento { get; set; } = BusinessConfig.Current?.Telefono ?? "Tel: (988) 123-4567";
    public string PieDePagina { get; set; } = BusinessConfig.Current?.MensajeTicket ?? "¡Gracias por su preferencia!";

    public Ticket()
    {
        CargarConfiguracion();
    }

    public Ticket(Pedido pedido, Cliente cliente, Usuario usuario, List<CarritoItem> items)
    {
        CargarConfiguracion();
        if (pedido != null)
        {
            IdPedido = pedido.IdPedido;
            Total = pedido.Total;
            MontoPagado = pedido.MontoPagado;
            SaldoPendiente = pedido.SaldoPendiente;
            Codigo = GenerarCodigo(pedido.IdPedido);
        }
        else
        {
            Codigo = GenerarCodigo(0);
        }

        if (cliente != null)
        {
            ClienteNombre = cliente.Nombre;
            TelefonoCliente = cliente.Telefono;
        }

        if (usuario != null)
        {
            AtendidoPor = usuario.Nombre;
        }

        if (items != null)
        {
            Items = items;
        }

        Fecha = DateTime.Now;
    }

    private void CargarConfiguracion()
    {
        if (BusinessConfig.Current != null)
        {
            Encabezado = !string.IsNullOrWhiteSpace(BusinessConfig.Current.NombreNegocio) ? BusinessConfig.Current.NombreNegocio : Encabezado;
            DireccionEstablecimiento = !string.IsNullOrWhiteSpace(BusinessConfig.Current.Direccion) ? BusinessConfig.Current.Direccion : DireccionEstablecimiento;
            TelefonoEstablecimiento = !string.IsNullOrWhiteSpace(BusinessConfig.Current.Telefono) ? BusinessConfig.Current.Telefono : TelefonoEstablecimiento;
            PieDePagina = !string.IsNullOrWhiteSpace(BusinessConfig.Current.MensajeTicket) ? BusinessConfig.Current.MensajeTicket : PieDePagina;
        }
    }

    /// <summary>
    /// Genera una clave de ticket estándar basada en fecha e ID de pedido.
    /// </summary>
    public static string GenerarCodigo(int idPedido)
    {
        return $"TCK-{DateTime.Now:yyyyMMdd}-{idPedido:D4}";
    }

    /// <summary>
    /// Genera la versión en texto plano estilo comprobante térmico.
    /// </summary>
    public string GenerarTextoFormateado()
    {
        var sb = new StringBuilder();
        sb.AppendLine("========================================");
        sb.AppendLine($"        {Encabezado.ToUpper()}");
        sb.AppendLine($"   {DireccionEstablecimiento}");
        sb.AppendLine($"       {TelefonoEstablecimiento}");
        sb.AppendLine("========================================");
        sb.AppendLine($"Ticket: {Codigo}");
        sb.AppendLine($"Fecha: {Fecha:dd/MM/yyyy HH:mm}");
        sb.AppendLine($"Cliente: {ClienteNombre}");
        if (!string.IsNullOrEmpty(TelefonoCliente))
            sb.AppendLine($"Teléfono: {TelefonoCliente}");
        sb.AppendLine($"Atendido por: {AtendidoPor}");
        sb.AppendLine("----------------------------------------");
        sb.AppendLine("Cant  Descripción            Subtotal");
        sb.AppendLine("----------------------------------------");

        foreach (var item in Items)
        {
            string desc = item.NombreServicio.Length > 20 
                ? item.NombreServicio.Substring(0, 20) 
                : item.NombreServicio.PadRight(20);
            sb.AppendLine($"{item.Cantidad,4:0.0} {desc} {item.Subtotal,8:C2}");
        }

        sb.AppendLine("----------------------------------------");
        sb.AppendLine($"TOTAL:              {Total,14:C2}");
        sb.AppendLine($"PAGADO:             {MontoPagado,14:C2}");
        sb.AppendLine($"SALDO PENDIENTE:    {SaldoPendiente,14:C2}");
        if (Cambio > 0)
            sb.AppendLine($"CAMBIO:             {Cambio,14:C2}");
        sb.AppendLine("========================================");
        sb.AppendLine($"     {PieDePagina}");
        sb.AppendLine("========================================");

        return sb.ToString();
    }
}