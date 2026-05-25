using System.Windows;
using System.Windows;
using LavanderiaApp.Modelos;

namespace LavanderiaApp;

public partial class MainWindow : Window
{
    private ClienteRepositorio _clienteRepo;
    private PedidoRepositorio _pedidoRepo;
    private PedidoServicio _pedidoServicio;

    public MainWindow()
    {
        InitializeComponent();
        _clienteRepo = new ClienteRepositorio();
        _pedidoRepo = new PedidoRepositorio();
        _pedidoServicio = new PedidoServicio();

        ActualizarTodo();
    }

    private void ActualizarTodo()
    {
        var listaClientes = _clienteRepo.ListarTodo();
        dgClientes.ItemsSource = listaClientes;
        cbClientes.ItemsSource = listaClientes;

        dgPedidos.ItemsSource = _pedidoRepo.ObtenerTodos();
    }

    private void BtnGuardar_Click(object sender, RoutedEventArgs e)
    {
        if (string.IsNullOrWhiteSpace(txtNombre.Text))
        {
            MessageBox.Show("Por favor, ingrese un nombre.");
            return;
        }

        var nuevoCliente = new Cliente
        {
            Nombre = txtNombre.Text,
            Telefono = txtTelefono.Text,
            Direccion = txtDireccion.Text
        };

        try
        {
            _clienteRepo.Guardar(nuevoCliente);
            MessageBox.Show("Cliente guardado con éxito.");

            // Limpiar campos y actualizar 
            txtNombre.Text = "";
            txtTelefono.Text = "";
            txtDireccion.Text = "";
            ActualizarTodo();
        }
        catch (System.Exception ex)
        {
            MessageBox.Show($"Error al guardar: {ex.Message}");
        }
    }

    private void BtnRegistrarPedido_Click(object sender, RoutedEventArgs e)
    {
        if (cbClientes.SelectedValue == null)
        {
            MessageBox.Show("Seleccione un cliente.");
            return;
        }

        /*
        if (dpFechaEntrega.SelectedDate == null)
        {
            MessageBox.Show("Seleccione una fecha de entrega.");
            return;
        }
        */

        decimal total = 0;
        if (!decimal.TryParse(txtTotalPedido.Text, out total))
        {
            MessageBox.Show("Ingrese un total válido.");
            return;
        }

        var nuevoPedido = new Pedido
        {
            IdCliente = (int)cbClientes.SelectedValue,
            IdUsuario = 1, // Por ahora asignamos el usuario 1 por defecto
            FechaRecepcion = System.DateTime.Now,
            FechaEntrega = null,
            Estado = "En espera",
            Total = total
        };

        string resultado = _pedidoServicio.RegistrarPedido(nuevoPedido);

        if (resultado == "Éxito")
        {
            MessageBox.Show("Pedido registrado correctamente.");
            txtTotalPedido.Text = "";
            ActualizarTodo();
        }
        else
        {
            MessageBox.Show(resultado);
        }
    }

    private void BtnEntregar_Click(object sender, RoutedEventArgs e)
    {
        if (dgPedidos.SelectedItem is Pedido pedidoSeleccionado)
        {
            if (pedidoSeleccionado.Estado == "Entregado")
            {
                MessageBox.Show("Este pedido ya fue entregado.");
                return;
            }

            string resultado = _pedidoServicio.EntregarPedido(pedidoSeleccionado.IdPedido);

            if (resultado == "Éxito")
            {
                MessageBox.Show("Pedido entregado y pagado correctamente.");
                ActualizarTodo();
            }
            else
            {
                MessageBox.Show(resultado);
            }
        }
        else
        {
            MessageBox.Show("Seleccione un pedido de la lista para entregar.");
        }
    }

    private void BtnCambiarEstado_Click(object sender, RoutedEventArgs e)
    {
        if (dgPedidos.SelectedItem is Pedido pedidoSeleccionado)
        {
            if (pedidoSeleccionado.Estado == "Entregado")
            {
                MessageBox.Show("No se puede cambiar el estado de un pedido ya entregado.");
                return;
            }

            string nuevoEstado = "";
            switch (pedidoSeleccionado.Estado)
            {
                case "En espera":
                    nuevoEstado = "Lavando";
                    break;
                case "Lavando":
                    nuevoEstado = "Terminado";
                    break;
                case "Terminado":
                    MessageBox.Show("El pedido ya está terminado. Use 'Entregar y Pagar' para finalizar.");
                    return;
                default:
                    nuevoEstado = "En espera";
                    break;
            }

            string resultado = _pedidoServicio.ActualizarEstado(pedidoSeleccionado.IdPedido, nuevoEstado);
            if (resultado == "Éxito")
            {
                ActualizarTodo();
            }
            else
            {
                MessageBox.Show(resultado);
            }
        }
        else
        {
            MessageBox.Show("Seleccione un pedido para cambiar su estado.");
        }
    }
}
