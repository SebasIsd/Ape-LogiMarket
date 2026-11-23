using LogiMarket.Application.Ports.Notifications;

namespace LogiMarket.Application.Notifications;

public class NotificadorSms : INotificador
{
    public string Canal => "SMS";
    public Task EnviarAsync(string destinatario, string asunto, string cuerpo, CancellationToken ct = default)
    {
        Console.WriteLine($"[SMS] Para: {destinatario} | {cuerpo}");
        return Task.CompletedTask;
    }
}