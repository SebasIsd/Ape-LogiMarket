using LogiMarket.Domain.Entities;

namespace LogiMarket.Application.Validation;

public class ClienteActivoValidator : IValidadorPedido
{
    private readonly IValidadorPedido? _siguiente;
    public ClienteActivoValidator(IValidadorPedido? siguiente = null) { _siguiente = siguiente; }

    public async Task<ResultadoValidacion> ValidarAsync(Pedido pedido, CancellationToken ct)
    {
        if (pedido.Cliente is null || !pedido.Cliente.Activo)
            return ResultadoValidacion.Falla("El cliente está inactivo o no existe.");
        return _siguiente is null ? ResultadoValidacion.Exito : await _siguiente.ValidarAsync(pedido, ct);
    }
}