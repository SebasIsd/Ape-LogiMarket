using System.Collections.Concurrent;

namespace LogiMarket.Application.Notifications;

/// <summary>
/// Singleton que almacena en memoria los avisos enviados por los notificadores.
/// Permite que la UI (Auditoría) los muestre visualmente.
/// </summary>
public class LogDeNotificaciones
{
    private readonly ConcurrentQueue<string> _cola = new();

    public void Registrar(string canal, string destinatario, string asunto)
    {
        _cola.Enqueue($"[{DateTime.Now:HH:mm:ss}] ({canal}) Para: {destinatario} | {asunto}");
    }

    public IReadOnlyList<string> ObtenerUltimos(int cantidad = 50)
    {
        return _cola.TakeLast(cantidad).ToList();
    }

    public void Limpiar() => _cola.Clear();
}