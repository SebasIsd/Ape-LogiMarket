namespace LogiMarket.Infrastructure.Catalog;
/// <summary>
/// Metadatos inmutables compartidos por todas las filas que usan el mismo SKU.
/// </summary>
public sealed class SkuMetadata
{
    public string Sku { get; }
    public string Nombre { get; }
    public bool EsKit { get; }
    public decimal PesoKg { get; }
    public SkuMetadata(
        string sku, 
        string nombre,
        bool esKit, 
        decimal pesoKg)
    { 
        Sku = sku; 
        Nombre = nombre; 
        EsKit = esKit; 
        PesoKg = pesoKg; 
    }
}