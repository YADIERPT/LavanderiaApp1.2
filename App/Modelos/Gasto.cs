using System;

namespace LavanderiaApp.Modelos;

public class Gasto
{
    public int IdGasto { get; set; }
    public decimal Monto { get; set; }
    public string Concepto { get; set; } = string.Empty;
    public DateTime Fecha { get; set; } = DateTime.Now;
}
