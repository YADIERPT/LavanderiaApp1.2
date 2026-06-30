using System.Collections.Generic;
using System.Windows;
using LavanderiaApp.Modelos;
using LavanderiaApp.Repositorios;
using LavanderiaApp.Servicios;

namespace LavanderiaApp;

public partial class MainWindow : Window
{
    private ClienteRepositorio _clienteRepo;
    private PedidoRepositorio _pedidoRepo;
    private PedidoServicio _pedidoServicio;
    private PagoServicio _pagoServicio;
    private ServicioRepositorio _servicioRepo;
    
    private List<CarritoItem> _carrito = new List<CarritoItem>();

    public MainWindow()
    {
        InitializeComponent();
        _clienteRepo = new ClienteRepositorio();
        _pedidoRepo = new PedidoRepositorio();
        _pedidoServicio = new PedidoServicio();
        _pagoServicio = new PagoServicio();
        _servicioRepo = new ServicioRepositorio();

        ActualizarTodo();
        CargarServicios();
        
        if (SessionManager.IsLoggedIn)
        {
            this.Title = $"Sistema Lavanderia - Usuario: {SessionManager.UsuarioActual.Nombre}";
        }
    }

    private void CargarServicios()
    {
        cbServicios.ItemsSource = _servicioRepo.ObtenerTodos();
    }

    private void ActualizarTodo()
    {
        var listaClientes = _clienteRepo.ListarTodo();
        dgClientes.ItemsSource = listaClientes;
        cbClientes.ItemsSource = listaClientes;

        dgPedidos.ItemsSource = _pedidoRepo.ObtenerTodos();
    }

    private void BtnAddServicio_Click(object sender, RoutedEventArgs e)
    {
        if (cbServicios.SelectedItem is Servicio servicioSeleccionado)
        {
            int cantidad = 1;
            int.TryParse(txtCantidadServicio.Text, out cantidad);

            var item = new CarritoItem
            {
                IdServicio = servicioSeleccionado.IdServicio,
                NombreServicio = servicioSeleccionado.Nombre,
                Cantidad = cantidad,
                PrecioUnitario = servicioSeleccionado.Precio,
                Subtotal = servicioSeleccionado.Precio * cantidad
            };

            _carrito.Add(item);
            ActualizarCarritoUI();
        }
        else
        {
            MessageBox.Show("Seleccione un servicio.");
        }
    }

    private void ActualizarCarritoUI()
    {
        dgCarrito.ItemsSource = null;
        dgCarrito.ItemsSource = _carrito;
        
        decimal total = 0;
        foreach (var item in _carrito) total += item.Subtotal;
        txtTotalPedido.Text = total.ToString("N2");
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

        if (_carrito.Count == 0)
        {
            MessageBox.Show("Añada al menos un servicio al carrito.");
            return;
        }
        decimal total = decimal.Parse(txtTotalPedido.Text);

        var nuevoPedido = new Pedido
        {
            IdCliente = (int)cbClientes.SelectedValue,
            IdUsuario = SessionManager.UsuarioActual?.IdUsuario ?? 1, 
            FechaRecepcion = System.DateTime.Now,
            FechaEntrega = null,
            Estado = "En espera",
            Total = total
        };

        var detalles = new List<DetallePedido>();
        foreach (var item in _carrito)
        {
            detalles.Add(new DetallePedido
            {
                IdServicio = item.IdServicio,
                Cantidad = item.Cantidad,
                PrecioUnitario = item.PrecioUnitario,
                Subtotal = item.Subtotal
            });
        }

        string resultado = _pedidoServicio.RegistrarPedido(nuevoPedido, detalles);

        if (resultado == "Éxito")
        {
            MessageBox.Show("Pedido registrado correctamente.");
            _carrito.Clear();
            ActualizarCarritoUI();
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

            // Aquí podríamos abrir una ventana para elegir el método de pago
            // Por simplicidad, usaremos Efectivo por defecto
            string resultado = _pagoServicio.ProcesarPago(pedidoSeleccionado.IdPedido, pedidoSeleccionado.Total, Pago.MetodoPago.Efectivo);

            if (resultado == "Éxito")
            {
                MessageBox.Show("Pedido entregado y pago registrado correctamente.");
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

public class CarritoItem
{
    public int IdServicio { get; set; }
    public string NombreServicio { get; set; }
    public double Cantidad { get; set; }
    public decimal PrecioUnitario { get; set; }
    public decimal Subtotal { get; set; }
}