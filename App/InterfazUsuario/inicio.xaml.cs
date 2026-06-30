using System.Windows;
using LavanderiaApp.Servicios;

namespace LavanderiaApp;

public partial class Inicio : Window
{
    private LoginServicio _loginServicio;

    public Inicio()
    {
        InitializeComponent();
        _loginServicio = new LoginServicio();
    }

    private void BtnLogin_Click(object sender, RoutedEventArgs e)
    {
        string usuario = txtUsuario.Text;
        string password = txtPassword.Password;

        if (_loginServicio.Login(usuario, password))
        {
            MainWindow main = new MainWindow();
            main.Show();
            this.Close();
        }
        else
        {
            MessageBox.Show("Usuario o contraseña incorrectos", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    private void BtnSalir_Click(object sender, RoutedEventArgs e)
    {
        Application.Current.Shutdown();
    }
}