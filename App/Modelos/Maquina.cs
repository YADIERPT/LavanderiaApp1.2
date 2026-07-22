namespace LavanderiaApp.Modelos;

/// <summary>
/// Representa el modelo de una lavadora o secadora en el sistema para control de ciclos y mantenimiento.
/// </summary>
public class Maquina
{
    public int IdMaquina { get; set; }
    private string? _nombre;
    public string Nombre
    {
        get => string.IsNullOrEmpty(_nombre) ? $"Unidad #{IdMaquina:02}" : _nombre;
        set => _nombre = value;
    }
    public string Status { get; set; } = "INACTIVA"; // "ACTIVA", "MANTENIMIENTO", "INACTIVA"
    public int CiclosOperados { get; set; }
    public int ProxMantenimientoCiclos { get; set; }
    public string Observacion { get; set; } = string.Empty;

    public string TimeRemainingText
    {
        get
        {
            if (Status.Equals("ACTIVA", StringComparison.OrdinalIgnoreCase)) return "Disponible";
            if (Status.Contains("EN USO", StringComparison.OrdinalIgnoreCase)) return "Máquina en Uso";
            if (Status.Equals("MANTENIMIENTO", StringComparison.OrdinalIgnoreCase) || Status.Equals("ALERT", StringComparison.OrdinalIgnoreCase)) return "Requiere Mantenimiento";
            return "Inactiva";
        }
    }
}
