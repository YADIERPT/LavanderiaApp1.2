using System;

namespace LavanderiaApp.Modelos;

public class Notificacion
{
    public int IdNotificacion { get; set; }
    public string Titulo { get; set; } = string.Empty;
    public string Mensaje { get; set; } = string.Empty;
    public string Tipo { get; set; } = "Alerta"; // Alerta, Info, Exito, Peligro
    public DateTime Fecha { get; set; } = DateTime.Now;
    public bool Leida { get; set; } = false;
}
