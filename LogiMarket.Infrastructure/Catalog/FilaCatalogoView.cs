using LogiMarket.Domain.ValueObjects;
namespace LogiMarket.Infrastructure.Catalog;
/// <summary>
/// Fila de vista: combina el flyweight (metadatos compartidos) con datos externos (stock, precio, cantidad).
/// </summary>
public sealed class FilaCatalogoView
{
    public SkuMetadata Metadata { get; }
    public int Stock { get; set; }
    public Money Precio { get; set; }
    public int CantidadSolicitada { get; set; }

    public FilaCatalogoView(SkuMetadata metadata, int stock, Money precio, int cantidad = 0)
    {
        Metadata = metadata;
        Stock = stock;
        Precio = precio;
        CantidadSolicitada = cantidad;
    }
}