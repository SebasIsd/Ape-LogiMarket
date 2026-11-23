using LogiMarket.Application.Ports.Persistence;
using LogiMarket.Domain.Entities;
using LogiMarket.Domain.Entities.Catalog;

namespace LogiMarket.Application.Validation;

public class StockValidator : IValidadorPedido
{
    private readonly IRepositorioCatalogo _catalogo;
    private readonly IValidadorPedido? _siguiente;

    public StockValidator(IRepositorioCatalogo catalogo, IValidadorPedido? siguiente = null)
    {
        _catalogo = catalogo;
        _siguiente = siguiente;
    }

    public async Task<ResultadoValidacion> ValidarAsync(Pedido pedido, CancellationToken ct)
    {
        var requeridoPorSku = new Dictionary<string, int>();

        foreach (var detalle in pedido.Detalles)
        {
            var elemento = await _catalogo.ObtenerPorSkuAsync(detalle.Sku, ct);
            if (elemento is null)
                return ResultadoValidacion.Falla($"SKU no encontrado: {detalle.Sku}");

            foreach (var (prod, cant) in elemento.ExpandirSkusFinales(detalle.Cantidad))
            {
                requeridoPorSku.TryGetValue(prod.Sku, out var actual);
                requeridoPorSku[prod.Sku] = actual + cant;
            }
        }

        foreach (var (sku, req) in requeridoPorSku)
        {
            var producto = await _catalogo.ObtenerPorSkuAsync(sku, ct) as ProductoSimple;
            if (producto is null || producto.Stock < req)
                return ResultadoValidacion.Falla($"Stock insuficiente para {sku}. Se requieren {req}.");
        }

        return _siguiente is null ? ResultadoValidacion.Exito : await _siguiente.ValidarAsync(pedido, ct);
    }
}