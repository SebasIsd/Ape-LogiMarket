using LogiMarket.Domain.Entities.Catalog;
using LogiMarket.Domain.ValueObjects;

namespace LogiMarket.Domain.Entities;

/// <summary>
/// Línea de pedido. Conserva la instantánea del precio y del SKU al momento
/// de la creación del pedido (los cambios posteriores del catálogo NO la afectan).
/// </summary>
public class PedidoDetalle
{
    public Guid Id { get; private set; }
    public Guid PedidoId { get; private set; }
    public string Sku { get; private set; } = default!;
    public string Nombre { get; private set; } = default!;
    public bool EsKit { get; private set; }
    public int Cantidad { get; private set; }
    public Money PrecioUnitarioInstantanea { get; private set; }

    protected PedidoDetalle() { }
    //constructor (interno)
    internal PedidoDetalle(string sku, string nombre, bool esKit, int cantidad, Money precioInstantanea)
    {
        Id = Guid.NewGuid();
        Sku = sku;
        Nombre = nombre;
        EsKit = esKit;
        Cantidad = cantidad;
        PrecioUnitarioInstantanea = precioInstantanea;
    }

    public PedidoDetalle(IElementoCatalogo elemento, int cantidad)
    {
        if (elemento is null) throw new Exceptions.DomainException("Elemento nulo.");
        if (cantidad <= 0) throw new Exceptions.DomainException("Cantidad inválida.");

        Id = Guid.NewGuid();
        Sku = elemento.Sku;
        Nombre = elemento.Nombre;
        EsKit = elemento.EsKit;
        Cantidad = cantidad;
        PrecioUnitarioInstantanea = elemento.CalcularPrecioTotal(1); // precio unitario snapshot
    }

    public Money SubtotalLinea => PrecioUnitarioInstantanea.Multiply(Cantidad);

    public void ActualizarCantidad(int nuevaCantidad)
    {
        if (nuevaCantidad <= 0) throw new Exceptions.DomainException("Cantidad inválida.");
        Cantidad = nuevaCantidad;
    }
}