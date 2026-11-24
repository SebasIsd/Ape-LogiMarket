using LogiMarket.Domain.Entities;

namespace LogiMarket.Application.UseCases.Devoluciones;

public class DevolucionDefectoHandler : ProcesarDevolucionTemplate
{
    private readonly Func<Pedido, CancellationToken, Task> _reintegrar;
    private readonly Func<Devolucion, CancellationToken, Task> _persistir;

    public DevolucionDefectoHandler(
        Func<Pedido, CancellationToken, Task> reintegrar,
        Func<Devolucion, CancellationToken, Task> persistir)
    {
        _reintegrar = reintegrar;
        _persistir = persistir;
    }

    protected override bool EvaluarCondicion(int dias) => dias <= 30;
    protected override Task ReintegrarStockAsync(Pedido pedido, CancellationToken ct) => _reintegrar(pedido, ct);
    protected override Task PersistirAsync(Devolucion dev, CancellationToken ct) => _persistir(dev, ct);
}
