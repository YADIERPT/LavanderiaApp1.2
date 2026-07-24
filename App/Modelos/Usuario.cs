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
    public string Turno { get; set; } = string.Empty;
    public string FechaContrato { get; set; } = string.Empty;
    public decimal Salario { get; set; } = 0.0m;
    public string Sucursal { get; set; } = string.Empty;
    public int Edad { get; set; } = 0;
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

    public virtual bool EsMaster => false;
    public virtual bool EsAdmin => false;
    public virtual bool EsEmpleado => true;

    public virtual bool PuedeGestionarPedidos => true;
    public virtual bool PuedeOperarMaquinas => true;
    public virtual bool PuedeGestionarInventario => true;
    public virtual bool PuedeVerFinanzas => false;

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