using LavanderiaApp.Modelos;
using LavanderiaApp.Repositorios;

namespace LavanderiaApp.Servicios;

public class LoginServicio
{
    private readonly UsuarioRepositorio _usuarioRepo;

    public LoginServicio(UsuarioRepositorio usuarioRepo)
    {
        _usuarioRepo = usuarioRepo;
    }

    // Destructor para cumplir con los requerimientos del proyecto
    ~LoginServicio()
    {
        System.Diagnostics.Debug.WriteLine("Destruyendo la instancia de LoginServicio y liberando recursos...");
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