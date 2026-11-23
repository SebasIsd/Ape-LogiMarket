using LogiMarket.Domain.Entities.Catalog;

namespace LogiMarket.Infrastructure.Persistence.Entities;

public class KitComponenteEntity
{
    public Guid Id { get; set; }
    public Guid KitPadreId { get; set; }
    public KitCompuesto KitPadre { get; set; } = default!;

    // Discriminador: "Producto" o "Kit"
    public string TipoElemento { get; set; } = default!;

    // FK nullable a Producto
    public Guid? ProductoId { get; set; }
    public ProductoSimple? Producto { get; set; }

    // FK nullable a Kit hijo
    public Guid? KitHijoId { get; set; }
    public KitCompuesto? KitHijo { get; set; }

    public int Cantidad { get; set; }

    public IElementoCatalogo ResolverElemento()
        => TipoElemento switch
        {
            "Producto" => Producto ?? throw new InvalidOperationException("Producto nulo"),
            "Kit" => KitHijo ?? throw new InvalidOperationException("Kit hijo nulo"),
            _ => throw new InvalidOperationException($"Tipo desconocido: {TipoElemento}")
        };
}