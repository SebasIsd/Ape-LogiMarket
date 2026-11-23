using LogiMarket.Domain.Entities;

namespace LogiMarket.Application.Ports.Persistence;

public interface IRepositorioMovimientos
{
    Task AgregarAsync(MovimientoInventario movimiento, CancellationToken ct = default);
    Task<IReadOnlyList<MovimientoInventario>> ListarPorSkuAsync(string sku, CancellationToken ct = default);
    Task<IReadOnlyList<MovimientoInventario>> ListarTodosAsync(CancellationToken ct = default);
}