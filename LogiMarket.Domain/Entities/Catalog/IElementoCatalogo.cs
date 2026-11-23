using LogiMarket.Domain.Entities;
using LogiMarket.Domain.Visitors;

namespace LogiMarket.Domain.Entities.Catalog;

/// <summary>
/// Componente del patrón Composite. Tanto ProductoSimple como KitCompuesto
/// implementan esta interfaz para tratarlos de forma uniforme.
/// </summary>
public interface IElementoCatalogo
{
    string Sku { get; }
    string Nombre { get; }
    bool EsKit { get; }

    /// <summary>Expande recursivamente hasta los SKU finales con sus cantidades.</summary>
    IEnumerable<(ProductoSimple Producto, int Cantidad)> ExpandirSkusFinales(int cantidad);

    /// <summary>Peso logístico total considerando la cantidad solicitada.</summary>
    decimal CalcularPesoTotal(int cantidad);

    /// <summary>Precio total según la instantánea del catálogo.</summary>
    ValueObjects.Money CalcularPrecioTotal(int cantidad);

    /// <summary>Punto de entrada para el patrón Visitor.</summary>
    void Accept(ICatalogoVisitor visitor);
}