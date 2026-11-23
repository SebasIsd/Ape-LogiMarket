using LogiMarket.Domain.Entities;
using LogiMarket.Domain.Entities.Catalog;
using LogiMarket.Domain.Enums;
using LogiMarket.Domain.Exceptions;

namespace LogiMarket.Application.Ports.Persistence;

public interface IRepositorioPedido
{
    Task<Pedido?> ObtenerPorIdAsync(Guid id, CancellationToken ct = default);
    Task<IReadOnlyList<Pedido>> ListarAsync(CancellationToken ct = default);
    Task AgregarAsync(Pedido pedido, CancellationToken ct = default);
    Task ActualizarAsync(Pedido pedido, CancellationToken ct = default);
    Task<bool> ExistePagoPorClaveAsync(string clave, CancellationToken ct = default);
    Task<bool> ExisteDevolucionParaPedidoAsync(Guid pedidoId, CancellationToken ct = default);
}