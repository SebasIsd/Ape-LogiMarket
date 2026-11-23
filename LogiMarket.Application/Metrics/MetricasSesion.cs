namespace LogiMarket.Application.Metrics;

/// <summary>
/// Singleton thread-safe con contadores de sesión.
/// La fuente de verdad sigue siendo la BD; esto es solo para UI en vivo.
/// </summary>
public sealed class MetricasSesion
{
    private static readonly Lazy<MetricasSesion> _instancia =
        new(() => new MetricasSesion(), LazyThreadSafetyMode.ExecutionAndPublication);

    public static MetricasSesion Instance => _instancia.Value;

    private int _pedidosConfirmados;
    private int _despachos;
    private int _devoluciones;
    private readonly object _lock = new();

    private MetricasSesion() { }

    public int PedidosConfirmados => Interlocked.CompareExchange(ref _pedidosConfirmados, 0, 0);
    public int Despachos => Interlocked.CompareExchange(ref _despachos, 0, 0);
    public int Devoluciones => Interlocked.CompareExchange(ref _devoluciones, 0, 0);

    public void IncrementarPedidos() => Interlocked.Increment(ref _pedidosConfirmados);
    public void IncrementarDespachos() => Interlocked.Increment(ref _despachos);
    public void IncrementarDevoluciones() => Interlocked.Increment(ref _devoluciones);

    public void Reiniciar()
    {
        lock (_lock)
        {
            _pedidosConfirmados = 0;
            _despachos = 0;
            _devoluciones = 0;
        }
    }
}