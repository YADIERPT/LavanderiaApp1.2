namespace LavanderiaApp.Modelos;

/// <summary>
/// Representa la entidad base de Usuario del sistema de lavandería.
/// Contiene la información general de acceso, perfil e identidad.
/// </summary>
public class Usuario
{
    public int IdUsuario { get; set; }
    public string Nombre { get; set; } = string.Empty;
    public string NombreUsuario { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
    public string Rol { get; set; } = string.Empty;
    public string Correo { get; set; } = string.Empty;
    public string Telefono { get; set; } = string.Empty;
    public DateTime FechaRegistro { get; set; } = DateTime.Now;
    public bool Activo { get; set; } = true;

    public Usuario() { }

    public Usuario(int idUsuario, string nombre, string nombreUsuario, string password, string rol)
    {
        IdUsuario = idUsuario;
        Nombre = nombre;
        NombreUsuario = nombreUsuario;
        Password = password;
        Rol = rol;
    }

    /// <summary>
    /// Comprueba si la contraseña ingresada coincide con la del usuario.
    /// </summary>
    public bool ValidarPassword(string password)
    {
        if (string.IsNullOrEmpty(password) || string.IsNullOrEmpty(Password))
            return false;
        return Password.Equals(password);
    }

    /// <summary>
    /// Determina si el usuario posee rol de Administrador.
    /// </summary>
    public bool EsAdmin => !string.IsNullOrEmpty(Rol) && 
        (Rol.Equals("Admin", StringComparison.OrdinalIgnoreCase) || 
         Rol.Equals("Administrador", StringComparison.OrdinalIgnoreCase));

    /// <summary>
    /// Determina si el usuario es un Empleado.
    /// </summary>
    public bool EsEmpleado => !EsAdmin;

    public bool PuedeGestionarPedidos => true;
    public bool PuedeOperarMaquinas => true;
    public bool PuedeGestionarInventario => true;
    public bool PuedeVerFinanzas => EsAdmin;

    /// <summary>
    /// Valida si el objeto cumple con las reglas básicas de negocio.
    /// </summary>
    public bool ValidarDatos(out List<string> errores)
    {
        errores = new List<string>();

        if (string.IsNullOrWhiteSpace(Nombre))
            errores.Add("El nombre completo es obligatorio.");

        if (string.IsNullOrWhiteSpace(NombreUsuario))
            errores.Add("El nombre de usuario es obligatorio.");

        if (string.IsNullOrWhiteSpace(Password))
            errores.Add("La contraseña es obligatoria.");

        return errores.Count == 0;
    }

    public override string ToString()
    {
        return $"{Nombre} ({NombreUsuario}) - Rol: {Rol}";
    }
}