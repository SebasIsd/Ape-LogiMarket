namespace LogiMarket.Application.Ports.Notifications;

public interface INotificador
{
    string Canal { get; }
    Task EnviarAsync(string destinatario, string asunto, string cuerpo, CancellationToken ct = default);
}

public interface INotificadorFactory
{
    INotificador Crear(string canal);
    IReadOnlyList<string> CanalesDisponibles { get; }
}