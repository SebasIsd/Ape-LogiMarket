using LogiMarket.Domain.Entities.Catalog;

namespace LogiMarket.Application.Ports.Persistence;

public interface IRepositorioCatalogo
{
    Task<IElementoCatalogo?> ObtenerPorSkuAsync(string sku, CancellationToken ct = default);
    Task<IReadOnlyList<IElementoCatalogo>> ListarTodoAsync(CancellationToken ct = default);
    Task<IReadOnlyList<ProductoSimple>> ListarProductosAsync(CancellationToken ct = default);
    Task<IReadOnlyList<KitCompuesto>> ListarKitsAsync(CancellationToken ct = default);
    Task AgregarProductoAsync(ProductoSimple producto, CancellationToken ct = default);
    Task AgregarKitAsync(KitCompuesto kit, CancellationToken ct = default);
    Task ActualizarProductoAsync(ProductoSimple producto, CancellationToken ct = default);
}