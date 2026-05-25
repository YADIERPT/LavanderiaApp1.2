using System.Windows;

namespace LavanderiaApp;

public partial class App : Application
{
    protected override void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);
        // Inicializamos la base de datos al arrancar la app
        DatabaseInitializer.Inicializar();
    }
}
