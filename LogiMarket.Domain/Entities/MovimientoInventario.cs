using LogiMarket.Domain.Enums;

namespace LogiMarket.Domain.Entities;

public class MovimientoInventario
{
    public Guid Id { get; private set; }
    public string Sku { get; private set; } = default!;
    public int Cantidad { get; private set; }
    public TipoMovimientoInventario Tipo { get; private set; }
    public string Motivo { get; private set; } = default!;
    public string? Referencia { get; private set; }
    public DateTime Fecha { get; private set; }

    protected MovimientoInventario() { }

    public MovimientoInventario(string sku, int cantidad, TipoMovimientoInventario tipo, string motivo, string? referencia)
    {
        if (string.IsNullOrWhiteSpace(sku)) throw new Exceptions.DomainException("SKU obligatorio.");
        if (cantidad <= 0) throw new Exceptions.DomainException("Cantidad inválida.");

        Id = Guid.NewGuid();
        Sku = sku;
        Cantidad = cantidad;
        Tipo = tipo;
        Motivo = motivo;
        Referencia = referencia;
        Fecha = DateTime.UtcNow;
    }
}