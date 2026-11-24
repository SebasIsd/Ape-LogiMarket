using LogiMarket.Application.Ports.Notifications;
using LogiMarket.Application.Ports.Persistence;
using Microsoft.Extensions.Logging;

namespace LogiMarket.Infrastructure.Outbox;

public class OutboxProcessor
{
    private readonly IRepositorioOutbox _outbox;
    private readonly INotificadorFactory _notificadorFactory;
    private readonly ILogger<OutboxProcessor> _log;

    public OutboxProcessor(IRepositorioOutbox outbox, INotificadorFactory notificadorFactory, ILogger<OutboxProcessor> log)
    {
        _outbox = outbox;
        _notificadorFactory = notificadorFactory;
        _log = log;
    }

    public async Task ProcesarPendientesAsync(string canal = "EMAIL", CancellationToken ct = default)
    {
        var pendientes = await _outbox.ObtenerPendientesAsync(50, ct);
        foreach (var ev in pendientes)
        {
            try
            {
                var notificador = _notificadorFactory.Crear(canal);
                await notificador.EnviarAsync(
                    "sistema@logimarket.local",
                    $"Evento: {ev.TipoEvento}",
                    ev.Payload,
                    ct);
                await _outbox.MarcarProcesadoAsync(ev.Id, ct);
                _log.LogInformation("Outbox procesado {Id} tipo {Tipo} por {Canal}", ev.Id, ev.TipoEvento, canal);
            }
            catch (Exception ex)
            {
                _log.LogError(ex, "Error procesando Outbox {Id}", ev.Id);
                await _outbox.IncrementarReintentoAsync(ev.Id, ct);
            }
        }
    }
}