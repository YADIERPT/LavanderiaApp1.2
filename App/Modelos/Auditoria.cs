using System;

namespace LavanderiaApp.Modelos;

public class Auditoria
{
    public int Id { get; set; }
    public DateTime Fecha { get; set; }
    public string Usuario { get; set; } = "";
    public string Modulo { get; set; } = "";
    public string Accion { get; set; } = "";
    public string Detalle { get; set; } = "";
}
