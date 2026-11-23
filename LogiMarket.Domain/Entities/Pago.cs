using LogiMarket.Domain.ValueObjects;

namespace LogiMarket.Domain.Entities;

public class Pago
{
    public Guid Id { get; private set; }
    public Guid PedidoId { get; private set; }
    public Money Monto { get; private set; }
    public string Referencia { get; private set; } = default!;
    public string ClaveIdempotencia { get; private set; } = default!;
    public string Proveedor { get; private set; } = default!;
    public DateTime Fecha { get; private set; }

    protected Pago() { }

    public Pago(Guid pedidoId, Money monto, string referencia, string claveIdempotencia, string proveedor)
    {
        if (pedidoId == Guid.Empty) throw new Exceptions.DomainException("PedidoId inválido.");
        if (monto.Amount <= 0) throw new Exceptions.DomainException("Monto inválido.");
        if (string.IsNullOrWhiteSpace(referencia)) throw new Exceptions.DomainException("Referencia obligatoria.");
        if (string.IsNullOrWhiteSpace(claveIdempotencia)) throw new Exceptions.DomainException("Clave idempotencia obligatoria.");

        Id = Guid.NewGuid();
        PedidoId = pedidoId;
        Monto = monto;
        Referencia = referencia;
        ClaveIdempotencia = claveIdempotencia;
        Proveedor = proveedor;
        Fecha = DateTime.UtcNow;
    }
}