namespace LavanderiaApp.Modelos;

/// <summary>
/// Representa a un usuario administrador con acceso total al sistema,
/// configuración, finanzas y administración de personal.
/// </summary>
public class Admin : Usuario
{
    public int NivelAcceso { get; set; } = 1;
    public bool Superusuario { get; set; } = true;
    public DateTime? UltimaSesion { get; set; }

    public Admin()
    {
        Rol = "Admin";
        Activo = true;
    }

    public Admin(int idUsuario, string nombre, string nombreUsuario, string password)
        : base(idUsuario, nombre, nombreUsuario, password, "Admin")
    {
        NivelAcceso = 1;
        Superusuario = true;
    }

    /// <summary>
    /// Verifica si el administrador posee permisos para ejecutar acciones avanzadas.
    /// </summary>
    public bool TienePermiso(string modulo)
    {
        if (!Activo) return false;
        if (Superusuario) return true;

        return modulo?.ToLower() switch
        {
            "finanzas" or "reportes" or "empleados" or "servicios" => NivelAcceso <= 2,
            "configuracion" => NivelAcceso == 1,
            _ => true
        };
    }
}
