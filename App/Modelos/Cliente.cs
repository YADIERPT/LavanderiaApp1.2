namespace LavanderiaApp.Modelos;

/// <summary>
/// Representa a un cliente del servicio de lavandería, sus datos de contacto,
/// clasificación y relación con su historial de pedidos.
/// </summary>
public class Cliente
{
    public int IdCliente { get; set; }
    public string Nombre { get; set; } = string.Empty;
    public string Telefono { get; set; } = string.Empty;
    public string Direccion { get; set; } = string.Empty;
    public string Correo { get; set; } = string.Empty;
    public string TipoCliente { get; set; } = "Cliente nuevo"; // "Cliente nuevo", "Cliente frecuente", "Miembro Premium", "Cliente mensual"
    public string Frecuencia { get; set; } = "N/A"; // "Semanal", "Diario", "Mensual", "N/A"
    public DateTime FechaRegistro { get; set; } = DateTime.Now;
    public bool Activo { get; set; } = true;
    public List<Pedido> HistorialPedidos { get; set; } = new();

    public Cliente() { }

    public Cliente(int idCliente, string nombre, string telefono, string direccion, string correo = "")
    {
        IdCliente = idCliente;
        Nombre = nombre;
        Telefono = telefono;
        Direccion = direccion;
        Correo = correo;
    }

    /// <summary>
    /// Genera automáticamente las iniciales del nombre del cliente.
    /// </summary>
    public string Iniciales
    {
        get
        {
            if (string.IsNullOrWhiteSpace(Nombre)) return "CL";
            var partes = Nombre.Split(' ', StringSplitOptions.RemoveEmptyEntries);
            if (partes.Length == 1) return partes[0].Substring(0, Math.Min(2, partes[0].Length)).ToUpper();
            return $"{partes[0][0]}{partes[1][0]}".ToUpper();
        }
    }

    /// <summary>
    /// Devuelve el número total de pedidos realizados por este cliente.
    /// </summary>
    public int TotalPedidosRealizados => HistorialPedidos?.Count ?? 0;

    /// <summary>
    /// Calcula el monto total acumulado gastado por el cliente en pedidos no cancelados.
    /// </summary>
    public decimal TotalGastado => HistorialPedidos?
        .Where(p => !string.Equals(p.Estado, "Cancelado", StringComparison.OrdinalIgnoreCase))
        .Sum(p => p.Total) ?? 0m;

    /// <summary>
    /// Devuelve la fecha del último pedido registrado o null si no posee historial.
    /// </summary>
    public DateTime? FechaUltimoPedido => HistorialPedidos?.MaxBy(p => p.FechaRecepcion)?.FechaRecepcion;

    /// <summary>
    /// Clasifica automáticamente al cliente en base al volumen de pedidos realizados.
    /// </summary>
    public void ActualizarTipoCliente()
    {
        int count = TotalPedidosRealizados;
        if (count >= 20)
        {
            TipoCliente = "Miembro Premium";
            Frecuencia = "Diario";
        }
        else if (count >= 5)
        {
            TipoCliente = "Cliente frecuente";
            Frecuencia = "Semanal";
        }
        else
        {
            TipoCliente = "Cliente nuevo";
            Frecuencia = "N/A";
        }
    }

    /// <summary>
    /// Valida que la información obligatoria del cliente esté presente y bien formada.
    /// </summary>
    public bool ValidarCliente(out List<string> errores)
    {
        errores = new List<string>();

        if (string.IsNullOrWhiteSpace(Nombre))
            errores.Add("El nombre del cliente es obligatorio.");

        if (string.IsNullOrWhiteSpace(Telefono))
            errores.Add("El teléfono de contacto es obligatorio.");

        if (string.IsNullOrWhiteSpace(Direccion))
            errores.Add("La dirección del cliente es obligatoria.");

        return errores.Count == 0;
    }

    public override string ToString()
    {
        return $"{Nombre} - Tel: {Telefono}";
    }
}
