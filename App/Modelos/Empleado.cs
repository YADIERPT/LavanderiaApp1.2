namespace LavanderiaApp.Modelos;

/// <summary>
/// Representa a un empleado del negocio de lavandería, con información contractual,
/// turno, asignación de puesto y estado operativo.
/// </summary>
public class Empleado : Usuario
{
    public string Codigo { get; set; } = string.Empty;
    public string Posicion { get; set; } = "Recepcionista";
    public string Turno { get; set; } = "Matutino";
    public string Estado { get; set; } = "ACTIVO"; // "ACTIVO", "EN DESCANSO", "INACTIVO"
    public decimal PagoMes { get; set; }
    public DateTime? FechaContrato { get; set; }
    public int? Edad { get; set; }
    public string Sucursal { get; set; } = "Sucursal Principal";
    public bool HasAvatarImage { get; set; } = false;

    public Empleado()
    {
        Rol = "Empleado";
    }

    public Empleado(int idUsuario, string nombre, string nombreUsuario, string password, string turno, string posicion = "Recepcionista")
        : base(idUsuario, nombre, nombreUsuario, password, "Empleado")
    {
        Turno = turno;
        Posicion = posicion;
    }

    /// <summary>
    /// Calcula el salario anual estimado del empleado a partir de su salario mensual.
    /// </summary>
    public decimal SalarioAnual => PagoMes * 12;

    /// <summary>
    /// Genera automáticamente las iniciales del nombre del empleado para avatares (p.ej. "Carlos Tec" -> "CT").
    /// </summary>
    public string Initials
    {
        get
        {
            if (string.IsNullOrWhiteSpace(Nombre)) return "EM";
            var partes = Nombre.Split(' ', StringSplitOptions.RemoveEmptyEntries);
            if (partes.Length == 1) return partes[0].Substring(0, Math.Min(2, partes[0].Length)).ToUpper();
            return $"{partes[0][0]}{partes[1][0]}".ToUpper();
        }
    }

    /// <summary>
    /// Indica si el empleado está disponible para atender o procesar pedidos.
    /// </summary>
    public bool EstaDisponible => Activo && Estado.Equals("ACTIVO", StringComparison.OrdinalIgnoreCase);

    /// <summary>
    /// Actualiza el estado operativo del empleado (ej. "ACTIVO", "EN DESCANSO", "INACTIVO").
    /// </summary>
    public void CambiarEstado(string nuevoEstado)
    {
        if (!string.IsNullOrWhiteSpace(nuevoEstado))
        {
            Estado = nuevoEstado.ToUpper().Trim();
        }
    }

    /// <summary>
    /// Realiza validaciones específicas del empleado.
    /// </summary>
    public bool ValidarEmpleado(out List<string> errores)
    {
        ValidarDatos(out errores);

        if (PagoMes < 0)
            errores.Add("El salario mensual no puede ser negativo.");

        if (Edad.HasValue && (Edad < 16 || Edad > 99))
            errores.Add("La edad ingresada no es válida.");

        return errores.Count == 0;
    }
}