using LogiMarket.Application.Ports.Notifications;

namespace LogiMarket.Application.Notifications;

public class NotificadorEmail : INotificador
{
    public string Canal => "EMAIL";
    public Task EnviarAsync(string destinatario, string asunto, string cuerpo, CancellationToken ct = default)
    {
        // Simulación: en Infrastructure se puede reemplazar por un logger o servicio real.
        Console.WriteLine($"[EMAIL] Para: {destinatario} | Asunto: {asunto} | {cuerpo}");
        return Task.CompletedTask;
    }
}