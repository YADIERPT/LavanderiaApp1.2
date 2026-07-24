using LavanderiaApp.Modelos;
using System;

namespace LavanderiaApp.Servicios;

public static class UsuarioFactory
{
    // Patrón de Diseño: Method Factory
    public static Usuario CrearUsuario(string rol)
    {
        if (string.IsNullOrWhiteSpace(rol))
            return new Empleado();

        string rolLimpio = rol.Trim();
        if (rolLimpio.Equals("Admin", StringComparison.OrdinalIgnoreCase) || 
            rolLimpio.Equals("Administrador", StringComparison.OrdinalIgnoreCase) ||
            rolLimpio.Equals("Master", StringComparison.OrdinalIgnoreCase))
        {
            return new Admin();
        }

        return new Empleado();
    }
}
