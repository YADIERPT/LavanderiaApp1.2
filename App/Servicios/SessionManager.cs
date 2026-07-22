using LavanderiaApp.Modelos;

namespace LavanderiaApp.Servicios;

public static class SessionManager
{
    public static Usuario UsuarioActual { get; set; }
    public static bool IsLoggedIn => UsuarioActual != null;
    public static decimal FondoCajaInicial { get; set; }
    public static int? LastSelectedOrderId { get; set; }
    public static int? LastSelectedClientId { get; set; }
    public static int? LastSelectedEmpleadoId { get; set; }
    public static int? LastSelectedMaquinaId { get; set; }

    public static void Logout()
    {
        UsuarioActual = null;
        FondoCajaInicial = 0;
        LastSelectedOrderId = null;
        LastSelectedClientId = null;
        LastSelectedEmpleadoId = null;
        LastSelectedMaquinaId = null;
    }
}