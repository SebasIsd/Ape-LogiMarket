using LogiMarket.Domain.Entities;

namespace LogiMarket.Application.Validation;

public class LineasValidator : IValidadorPedido
{
    private readonly IValidadorPedido? _siguiente;
    public LineasValidator(IValidadorPedido? siguiente = null) { _siguiente = siguiente; }

    public async Task<ResultadoValidacion> ValidarAsync(Pedido pedido, CancellationToken ct)
    {
        if (pedido.Detalles.Count == 0)
            return ResultadoValidacion.Falla("El pedido no tiene líneas.");
        if (pedido.Detalles.Any(d => d.Cantidad <= 0))
            return ResultadoValidacion.Falla("Cantidad inválida en una línea.");
        return _siguiente is null ? ResultadoValidacion.Exito : await _siguiente.ValidarAsync(pedido, ct);
    }
}