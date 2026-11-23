using LogiMarket.Domain.Exceptions;
using LogiMarket.Domain.ValueObjects;
using LogiMarket.Domain.Visitors;

namespace LogiMarket.Domain.Entities.Catalog;

public class ProductoSimple : IElementoCatalogo
{
    public Guid Id { get; private set; }
    public string Sku { get; private set; } = default!;
    public string Nombre { get; private set; } = default!;
    public Money Precio { get; private set; }
    public int Stock { get; private set; }
    public decimal PesoKg { get; private set; }
    public bool EsKit => false;

    // Constructor protegido para EF / deserialización (no rompe invariantes porque se usa internamente)
    protected ProductoSimple() { }

    public ProductoSimple(string sku, string nombre, Money precio, int stock, decimal pesoKg)
    {
        if (string.IsNullOrWhiteSpace(sku))
            throw new DomainException("El SKU es obligatorio.");
        if (string.IsNullOrWhiteSpace(nombre))
            throw new DomainException("El nombre es obligatorio.");
        if (precio.Amount < 0)
            throw new DomainException("El precio no puede ser negativo.");
        if (stock < 0)
            throw new DomainException("El stock no puede ser negativo.");
        if (pesoKg < 0)
            throw new DomainException("El peso no puede ser negativo.");

        Id = Guid.NewGuid();
        Sku = sku;
        Nombre = nombre;
        Precio = precio;
        Stock = stock;
        PesoKg = pesoKg;
    }

    public void ActualizarPrecio(Money nuevoPrecio)
    {
        if (nuevoPrecio.Amount < 0)
            throw new DomainException("El precio no puede ser negativo.");
        Precio = nuevoPrecio;
    }

    public void AjustarStock(int delta)
    {
        var nuevo = Stock + delta;
        if (nuevo < 0)
            throw new StockInsuficienteException(Sku);
        Stock = nuevo;
    }

    public void DescontarStock(int cantidad)
    {
        if (cantidad <= 0)
            throw new DomainException("La cantidad a descontar debe ser positiva.");
        if (cantidad > Stock)
            throw new StockInsuficienteException(Sku);
        Stock -= cantidad;
    }

    public void ReintegrarStock(int cantidad)
    {
        if (cantidad <= 0)
            throw new DomainException("La cantidad a reintegrar debe ser positiva.");
        Stock += cantidad;
    }

    // Composite
    public IEnumerable<(ProductoSimple Producto, int Cantidad)> ExpandirSkusFinales(int cantidad)
    {
        yield return (this, cantidad);
    }

    public decimal CalcularPesoTotal(int cantidad) => PesoKg * cantidad;

    public Money CalcularPrecioTotal(int cantidad) => Precio.Multiply(cantidad);

    public void Accept(ICatalogoVisitor visitor) => visitor.VisitProducto(this);
}