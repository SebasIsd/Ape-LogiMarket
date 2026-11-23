using LogiMarket.Domain.ValueObjects;

namespace LogiMarket.Infrastructure.Catalog;

/// <summary>
/// Flyweight Factory: comparte instancias de SkuMetadata para ahorrar memoria
/// cuando se muestran miles de filas en DataGridView.
/// </summary>
public sealed class CatalogoVisualFlyweightFactory
{
    private readonly Dictionary<string, SkuMetadata> _cache = new();

    public SkuMetadata Obtener(string sku, string nombre, bool esKit, decimal pesoKg)
    {
        if (_cache.TryGetValue(sku, out var existente)) return existente;
        var nuevo = new SkuMetadata(sku, nombre, esKit, pesoKg);
        _cache[sku] = nuevo;
        return nuevo;
    }

    public int TotalEnCache => _cache.Count;
    public void Limpiar() => _cache.Clear();
}