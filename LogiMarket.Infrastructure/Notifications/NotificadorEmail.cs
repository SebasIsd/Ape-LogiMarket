using LogiMarket.Application.Notifications;
using LogiMarket.Application.Ports.Notifications;

namespace LogiMarket.Infrastructure.Notifications;

public class NotificadorEmail : INotificador
{
    private readonly LogDeNotificaciones _log;
    public string Canal => "EMAIL";

    public NotificadorEmail(LogDeNotificaciones log) { _log = log; }

    public Task EnviarAsync(string destinatario, string asunto, string cuerpo, CancellationToken ct = default)
    {
        _log.Registrar(Canal, destinatario, asunto); // ← Aquí se guarda para la UI
        return Task.CompletedTask;
    }
}