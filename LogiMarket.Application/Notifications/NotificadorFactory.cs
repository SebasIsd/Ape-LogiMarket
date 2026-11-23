using LogiMarket.Application.Ports.Notifications;
using LogiMarket.Domain.Exceptions;

namespace LogiMarket.Application.Notifications;

/// <summary>
/// Factory Method: cada creador retorna un notificador concreto.
/// </summary>
public class NotificadorFactory : INotificadorFactory
{
    public IReadOnlyList<string> CanalesDisponibles => new[] { "EMAIL", "SMS" };

    public INotificador Crear(string canal) => canal.ToUpperInvariant() switch
    {
        "EMAIL" => new NotificadorEmail(),
        "SMS" => new NotificadorSms(),
        _ => throw new DomainException($"Canal no soportado: {canal}")
    };
}