using System.Windows;
using Microsoft.Extensions.DependencyInjection;
using LavanderiaApp.Servicios;
using LavanderiaApp.Repositorios;

namespace LavanderiaApp;

public partial class App : Application
{
    public static IServiceProvider Services { get; private set; } = null!;

    protected override void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);
        
        // Inicializamos la base de datos al arrancar la app
        DatabaseInitializer.Inicializar();

        // Configuración de Inyección de Dependencias para Blazor Hybrid
        var serviceCollection = new ServiceCollection();
        serviceCollection.AddWpfBlazorWebView();

        // Registrar servicios del negocio y repositorios
        serviceCollection.AddSingleton<LoginServicio>();
        serviceCollection.AddSingleton<ClienteRepositorio>();
        serviceCollection.AddSingleton<PedidoRepositorio>();
        serviceCollection.AddSingleton<ServicioRepositorio>();
        serviceCollection.AddSingleton<UsuarioRepositorio>();
        serviceCollection.AddSingleton<PagoRepositorio>();
        serviceCollection.AddSingleton<InventarioRepositorio>();
        serviceCollection.AddSingleton<MaquinaRepositorio>();
        serviceCollection.AddSingleton<PedidoServicio>();
        serviceCollection.AddSingleton<PagoServicio>();

#if DEBUG
        serviceCollection.AddBlazorWebViewDeveloperTools();
#endif

        Services = serviceCollection.BuildServiceProvider();
    }
}
