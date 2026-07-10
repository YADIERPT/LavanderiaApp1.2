using System.Text;

namespace LavanderiaApp.Modelos;

/// <summary>
/// Representa un reporte consolidador de métricas financieras, operativas
/// y de rendimiento para la lavandería.
/// </summary>
public class Reporte
{
    public int IdReporte { get; set; }
    public string TipoReporte { get; set; } = "Diario"; // "Diario", "Semanal", "Mensual", "Financiero"
    public DateTime FechaGeneracion { get; set; } = DateTime.Now;
    public DateTime FechaInicio { get; set; } = DateTime.Today;
    public DateTime FechaFin { get; set; } = DateTime.Now;
    public decimal TotalIngresos { get; set; }
    public decimal TotalCostoInsumos { get; set; }
    public decimal UtilidadNeto => TotalIngresos - TotalCostoInsumos;
    public int TotalPedidos { get; set; }
    public int PedidosCompletados { get; set; }
    public int PedidosPendientes { get; set; }
    public int PedidosCancelados { get; set; }
    public decimal PromedioVentaPorPedido { get; set; }
    public string GeneradoPor { get; set; } = "Sistema";
    public List<Pedido> PedidosIncluidos { get; set; } = new();

    public Reporte() { }

    public Reporte(string tipoReporte, DateTime fechaInicio, DateTime fechaFin, string generadoPor = "Sistema")
    {
        TipoReporte = tipoReporte;
        FechaInicio = fechaInicio;
        FechaFin = fechaFin;
        GeneradoPor = generadoPor;
        FechaGeneracion = DateTime.Now;
    }

    /// <summary>
    /// Procesa una lista de pedidos y calcula de manera automática todas las métricas del reporte.
    /// </summary>
    public void CalcularMetricas(IEnumerable<Pedido> pedidos, IEnumerable<Pago> pagos)
    {
        if (pedidos == null) return;
        var pagosList = pagos?.ToList() ?? new List<Pago>();

        var filtrados = pedidos
            .Where(p => p.FechaRecepcion >= FechaInicio && p.FechaRecepcion <= FechaFin)
            .ToList();

        PedidosIncluidos = filtrados;
        TotalPedidos = filtrados.Count;

        TotalIngresos = pagosList
            .Where(p => p.FechaPago >= FechaInicio && p.FechaPago <= FechaFin)
            .Sum(p => p.MontoPago);

        TotalCostoInsumos = filtrados
            .Where(p => !string.Equals(p.Estado, "Cancelado", StringComparison.OrdinalIgnoreCase))
            .Sum(p => p.CostoInsumos);

        PedidosCompletados = filtrados
            .Count(p => string.Equals(p.Estado, "Entregado", StringComparison.OrdinalIgnoreCase) || 
                        string.Equals(p.Estado, "Listo", StringComparison.OrdinalIgnoreCase));

        PedidosPendientes = filtrados
            .Count(p => string.Equals(p.Estado, "En espera", StringComparison.OrdinalIgnoreCase) || 
                        string.Equals(p.Estado, "En proceso", StringComparison.OrdinalIgnoreCase));

        PedidosCancelados = filtrados
            .Count(p => string.Equals(p.Estado, "Cancelado", StringComparison.OrdinalIgnoreCase));

        int validezConVentas = TotalPedidos - PedidosCancelados;
        PromedioVentaPorPedido = validezConVentas > 0 ? TotalIngresos / validezConVentas : 0m;
    }

    /// <summary>
    /// Genera un resumen ejecutivo en formato de texto.
    /// </summary>
    public string GenerarResumenTexto()
    {
        var sb = new StringBuilder();
        sb.AppendLine($"--- REPORTE {TipoReporte.ToUpper()} ---");
        sb.AppendLine($"Periodo: {FechaInicio:dd/MM/yyyy} a {FechaFin:dd/MM/yyyy}");
        sb.AppendLine($"Generado el: {FechaGeneracion:dd/MM/yyyy HH:mm} por {GeneradoPor}");
        sb.AppendLine($"----------------------------------------");
        sb.AppendLine($"Total Pedidos:          {TotalPedidos}");
        sb.AppendLine($"Pedidos Completados:    {PedidosCompletados}");
        sb.AppendLine($"Pedidos Pendientes:     {PedidosPendientes}");
        sb.AppendLine($"Pedidos Cancelados:     {PedidosCancelados}");
        sb.AppendLine($"Total Ingresos:         {TotalIngresos:C2}");
        sb.AppendLine($"Promedio por Pedido:    {PromedioVentaPorPedido:C2}");
        sb.AppendLine($"----------------------------------------");
        return sb.ToString();
    }
}