using LavanderiaApp.Modelos;
using LavanderiaApp.Repositorios;

namespace LavanderiaApp.Servicios;

public class ClienteServicio
{
    public void RegistrarCliente(Cliente cliente)
    {
        // VALIDACIONES

        if (string.IsNullOrWhiteSpace(cliente.Nombre))
        {
            throw new Exception(
                "El nombre es obligatorio.");
        }

        if (string.IsNullOrWhiteSpace(cliente.Telefono))
        {
            throw new Exception(
                "El teléfono es obligatorio.");
        }

        if (string.IsNullOrWhiteSpace(cliente.Direccion))
        {
            throw new Exception(
                "La dirección es obligatoria.");
        }


        // REPOSITORIO

        ClienteRepositorio repo =
            new ClienteRepositorio();

        repo.Guardar(cliente);
    }
}