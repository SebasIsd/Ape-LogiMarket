namespace LogiMarket.Domain.Entities.Catalog;

public class KitComponente
{
    public Guid Id { get; private set; }
    public IElementoCatalogo Elemento { get; private set; } = default!; 
    public int Cantidad { get; }

    protected KitComponente() { } // EF

    public KitComponente(IElementoCatalogo elemento, int cantidad)
    {
        Elemento = elemento ?? throw new ArgumentNullException(nameof(elemento));
        if (cantidad <= 0)
            throw new Domain.Exceptions.DomainException("Cantidad inválida.");
        Cantidad = cantidad;
        Id = Guid.NewGuid();
    }
}