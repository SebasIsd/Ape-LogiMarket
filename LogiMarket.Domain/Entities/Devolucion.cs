using LogiMarket.Domain.Enums;
using LogiMarket.Domain.ValueObjects;

namespace LogiMarket.Domain.Entities;

public class Devolucion
{
    public Guid Id { get; private set; }
    public Guid PedidoId { get; private set; }
    public MotivoDevolucion Motivo { get; private set; }
    public Money Reembolso { get; private set; }
    public DateTime FechaSolicitud { get; private set; }
    public bool Aceptada { get; private set; }

    protected Devolucion() { }

    public Devolucion(Guid pedidoId, MotivoDevolucion motivo, Money reembolso, bool aceptada)
    {
        if (pedidoId == Guid.Empty) throw new Exceptions.DomainException("PedidoId inválido.");

        Id = Guid.NewGuid();
        PedidoId = pedidoId;
        Motivo = motivo;
        Reembolso = reembolso;
        Aceptada = aceptada;
        FechaSolicitud = DateTime.UtcNow;
    }
}