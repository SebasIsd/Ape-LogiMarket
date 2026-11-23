using LogiMarket.Domain.Exceptions;
using LogiMarket.Domain.ValueObjects;
using LogiMarket.Domain.Visitors;

namespace LogiMarket.Domain.Entities.Catalog;

public class KitCompuesto : IElementoCatalogo
{
    public Guid Id { get; private set; }
    public string Sku { get; private set; } = default!;
    public string Nombre { get; private set; } = default!;
    public bool EsKit => true;

    private readonly List<KitComponente> _componentes = new();
    public IReadOnlyCollection<KitComponente> Componentes => _componentes.AsReadOnly();

    protected KitCompuesto() { }

    public KitCompuesto(string sku, string nombre)
    {
        if (string.IsNullOrWhiteSpace(sku))
            throw new DomainException("El SKU del kit es obligatorio.");
        if (string.IsNullOrWhiteSpace(nombre))
            throw new DomainException("El nombre del kit es obligatorio.");

        Id = Guid.NewGuid();
        Sku = sku;
        Nombre = nombre;
    }

    public void AgregarComponente(IElementoCatalogo elemento, int cantidad)
    {
        if (elemento is null)
            throw new DomainException("El componente no puede ser nulo.");
        if (cantidad <= 0)
            throw new DomainException("La cantidad del componente debe ser positiva.");

        // Anti-ciclo: un kit no puede contenerse a sí mismo, directa o indirectamente
        if (elemento is KitCompuesto kit && ContieneA(this, kit))
            throw new ReferenciaCircularException(Sku);

        _componentes.Add(new KitComponente(elemento, cantidad));
    }

    private static bool ContieneA(KitCompuesto contenedor, KitCompuesto candidato)
    {
        if (candidato.Sku == contenedor.Sku) return true;
        foreach (var c in candidato.Componentes)
        {
            if (c.Elemento is KitCompuesto k && ContieneA(contenedor, k))
                return true;
        }
        return false;
    }

    // Composite: expansión recursiva hasta productos simples
    public IEnumerable<(ProductoSimple Producto, int Cantidad)> ExpandirSkusFinales(int cantidad)
    {
        foreach (var componente in _componentes)
        {
            var total = componente.Cantidad * cantidad;
            foreach (var exp in componente.Elemento.ExpandirSkusFinales(total))
                yield return exp;
        }
    }

    public decimal CalcularPesoTotal(int cantidad)
    {
        decimal total = 0m;
        foreach (var c in _componentes)
            total += c.Elemento.CalcularPesoTotal(c.Cantidad * cantidad);
        return total;
    }

    public Money CalcularPrecioTotal(int cantidad)
    {
        var total = Money.Zero;
        foreach (var c in _componentes)
            total = total.Add(c.Elemento.CalcularPrecioTotal(c.Cantidad * cantidad));
        return total;
    }

    public void Accept(ICatalogoVisitor visitor) => visitor.VisitKit(this);
}