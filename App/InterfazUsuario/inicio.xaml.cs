using System.Windows;
using System.Windows.Input;

namespace LavanderiaApp;

public partial class Inicio : Window
{
    public Inicio()
    {
        InitializeComponent();
        
        // Inicializamos los servicios de BlazorWebView
        blazorWebView.Services = App.Services;
    }

    // Permite arrastrar la ventana sin bordes al hacer clic y arrastrar en la parte superior
    private void Window_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
    {
        if (e.LeftButton == MouseButtonState.Pressed)
        {
            this.DragMove();
        }
    }

    // Minimiza la ventana
    private void BtnMinimizar_Click(object sender, RoutedEventArgs e)
    {
        this.WindowState = WindowState.Minimized;
    }

    // Maximiza / Restaura la ventana
    private void BtnMaximizar_Click(object sender, RoutedEventArgs e)
    {
        this.WindowState = this.WindowState == WindowState.Maximized 
            ? WindowState.Normal 
            : WindowState.Maximized;
    }

    // Cierra la aplicación
    private void BtnSalir_Click(object sender, RoutedEventArgs e)
    {
        Application.Current.Shutdown();
    }
}