using System.Windows;
using LavanderiaApp.Servicios;


namespace LavanderiaApp;

public partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();
        
        // Inicializar los servicios de BlazorWebView para renderizar componentes Razor
        blazorWebView.Services = App.Services;
        
        if (SessionManager.IsLoggedIn)
        {
            this.Title = $"Lavandería Pro - Panel de Administración: {SessionManager.UsuarioActual.Nombre}";
        }
    }
}