using LavanderiaApp.Modelos;

namespace LavanderiaApp.Servicios;

public static class SessionManager
{
    public static Usuario UsuarioActual { get; set; }
    public static bool IsLoggedIn => UsuarioActual != null;

    public static void Logout()
    {
        UsuarioActual = null;
    }
}