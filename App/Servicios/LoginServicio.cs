using LavanderiaApp.Modelos;
using LavanderiaApp.Repositorios;

namespace LavanderiaApp.Servicios;

public class LoginServicio
{
    private UsuarioRepositorio _usuarioRepo;

    public LoginServicio()
    {
        _usuarioRepo = new UsuarioRepositorio();
    }

    public bool Login(string nombreUsuario, string password)
    {
        var usuario = _usuarioRepo.ObtenerPorNombreUsuario(nombreUsuario);
        if (usuario != null && usuario.Password == password)
        {
            SessionManager.UsuarioActual = usuario;
            return true;
        }
        return false;
    }
}