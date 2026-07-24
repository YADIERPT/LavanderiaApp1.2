using LavanderiaApp.Modelos;
using System.Collections.Generic;

namespace LavanderiaApp.Repositorios;

public interface IPagoRepositorio
{
    void Guardar(Pago pago);
    List<Pago> ObtenerTodos();
}
