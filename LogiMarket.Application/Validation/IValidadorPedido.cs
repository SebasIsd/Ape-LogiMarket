using LogiMarket.Domain.Entities;

namespace LogiMarket.Application.Validation;

public interface IValidadorPedido
{
    Task<ResultadoValidacion> ValidarAsync(Pedido pedido, CancellationToken ct = default);
}