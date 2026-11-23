using LogiMarket.Domain.Entities;

namespace LogiMarket.Application.UseCases.Devoluciones;
public class DevolucionDefectoHandler : ProcesarDevolucionTemplate
{
    private readonly Action<Pedido> _reintegrar;
    private readonly Func<Devolucion, CancellationToken, Task> _persistir;

    public DevolucionDefectoHandler(Action<Pedido> reintegrar, Func<Devolucion, CancellationToken, Task> persistir)
    {
        _reintegrar = reintegrar;
        _persistir = persistir;
    }

    protected override bool EvaluarCondicion(int dias) => dias <= 30;
    protected override void ReintegrarStock(Pedido pedido) => _reintegrar(pedido);
    protected override Task PersistirAsync(Devolucion dev, CancellationToken ct) => _persistir(dev, ct);
}