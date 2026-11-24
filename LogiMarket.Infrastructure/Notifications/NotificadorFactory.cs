using LogiMarket.Application.Notifications;
using LogiMarket.Application.Ports.Notifications;
using LogiMarket.Domain.Exceptions;

namespace LogiMarket.Infrastructure.Notifications;

public class NotificadorFactory : INotificadorFactory
{
    private readonly LogDeNotificaciones _log;
    public NotificadorFactory(LogDeNotificaciones log) { _log = log; }

    public IReadOnlyList<string> CanalesDisponibles => new[] { "EMAIL", "SMS" };

    public INotificador Crear(string canal) => canal.ToUpperInvariant() switch
    {
        "EMAIL" => new NotificadorEmail(_log),
        "SMS" => new NotificadorSms(_log),
        _ => throw new DomainException($"Canal no soportado: {canal}")
    };
}