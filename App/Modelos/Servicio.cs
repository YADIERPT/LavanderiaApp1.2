namespace LavanderiaApp.Modelos;

/// <summary>
/// Representa un servicio ofrecido por la lavandería (ej. Lavado General, Secado, Planchado),
/// especificando su costo por unidad de medida y tiempo estimado de atención.
/// </summary>
public class Servicio
{  
    public int IdServicio { get; set; }
    public string Nombre { get; set; } = string.Empty;
    public string Descripcion { get; set; } = string.Empty;
    public decimal Precio { get; set; }
    public int TiempoEstimado { get; set; } // En minutos
    public string UnidadMedida { get; set; } = "Kg"; // "Kg", "Unidad", "Carga", etc.
    public bool Activo { get; set; } = true;

    public Servicio() { }

    public Servicio(int idServicio, string nombre, string descripcion, decimal precio, int tiempoEstimado, string unidadMedida = "Kg")
    {
        IdServicio = idServicio;
        Nombre = nombre;
        Descripcion = descripcion;
        Precio = precio;
        TiempoEstimado = tiempoEstimado;
        UnidadMedida = unidadMedida;
    }

    /// <summary>
    /// Calcula el subtotal para una cantidad específica consumida de este servicio.
    /// </summary>
    public decimal CalcularSubtotal(double cantidad)
    {
        if (cantidad <= 0) return 0m;
        return (decimal)cantidad * Precio;
    }

    /// <summary>
    /// Devuelve el tiempo estimado formateado de manera legible (p.ej. "2 h 0 min" o "45 min").
    /// </summary>
    public string TiempoEstimadoFormateado
    {
        get
        {
            if (TiempoEstimado <= 0) return "Inmediato";
            int horas = TiempoEstimado / 60;
            int minutos = TiempoEstimado % 60;

            if (horas > 0 && minutos > 0)
                return $"{horas}h {minutos}m";
            if (horas > 0)
                return $"{horas}h";
            return $"{minutos}m";
        }
    }

    /// <summary>
    /// Valida que el servicio cumpla con los requisitos del catálogo.
    /// </summary>
    public bool ValidarServicio(out List<string> errores)
    {
        errores = new List<string>();

        if (string.IsNullOrWhiteSpace(Nombre))
            errores.Add("El nombre del servicio es obligatorio.");

        if (Precio <= 0)
            errores.Add("El precio del servicio debe ser mayor a 0.");

        if (TiempoEstimado < 0)
            errores.Add("El tiempo estimado no puede ser negativo.");

        return errores.Count == 0;
    }

    public override string ToString()
    {
        return $"{Nombre} ({UnidadMedida}) - {Precio:C2}";
    }
}