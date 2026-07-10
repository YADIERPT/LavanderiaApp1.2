using System;

namespace LavanderiaApp.Modelos;

public class CorteCaja
{
    public int IdCorte { get; set; }
    public DateTime Fecha { get; set; } = DateTime.Now;
    public decimal EfectivoReportado { get; set; }
    public decimal EfectivoEsperado { get; set; }
    public decimal Diferencia { get; set; }
    public string Empleado { get; set; } = string.Empty;
}
