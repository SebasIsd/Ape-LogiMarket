namespace LogiMarket.Domain.Entities;

public class Despacho
{
    public Guid Id { get; private set; }
    public Guid PedidoId { get; private set; }
    public string Guia { get; private set; } = default!;
    public string TipoEnvio { get; private set; } = default!;
    public int PlazoDias { get; private set; }
    public DateTime FechaDespacho { get; private set; }
    public DateTime FechaEstimadaEntrega { get; private set; }

    protected Despacho() { }

    public Despacho(Guid pedidoId, string guia, string tipoEnvio, int plazoDias)
    {
        if (pedidoId == Guid.Empty) throw new Exceptions.DomainException("PedidoId inválido.");
        if (string.IsNullOrWhiteSpace(guia)) throw new Exceptions.DomainException("Guía obligatoria.");

        Id = Guid.NewGuid();
        PedidoId = pedidoId;
        Guia = guia;
        TipoEnvio = tipoEnvio;
        PlazoDias = plazoDias;
        FechaDespacho = DateTime.UtcNow;
        FechaEstimadaEntrega = DateTime.UtcNow.AddDays(plazoDias);
    }
}