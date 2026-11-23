using LogiMarket.Application.Events;
using LogiMarket.Application.Ports.Persistence;
using LogiMarket.Domain.Entities;
using System.Text.Json;

namespace LogiMarket.Infrastructure.Outbox;

public class OutboxPublisher : IPublisher
{
    private readonly IRepositorioOutbox _outbox;
    public OutboxPublisher(IRepositorioOutbox outbox) { _outbox = outbox; }

    public async Task PublishAsync(IDominioEvento evento, CancellationToken ct = default)
    {
        var payload = JsonSerializer.Serialize(new
        {
            evento.EventoId,
            evento.OcurridoEn,
            evento.Tipo,
            Data = evento
        });
        await _outbox.AgregarAsync(new EventoOutbox(evento.Tipo, payload), ct);
    }
}