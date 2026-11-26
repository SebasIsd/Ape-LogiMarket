using LogiMarket.Application.Events;
using LogiMarket.Application.Metrics;

namespace LogiMarket.Infrastructure.Events.Subscribers;

public class MetricasSubscriber : ISubscriber<PedidoPagadoEvent>
{
    private readonly MetricasSesion _metricas;
    public MetricasSubscriber(MetricasSesion metricas) { _metricas = metricas; }

    public Task HandleAsync(PedidoPagadoEvent evento, CancellationToken ct = default)
    {
        _metricas.IncrementarPedidos();
        return Task.CompletedTask;
    }
}