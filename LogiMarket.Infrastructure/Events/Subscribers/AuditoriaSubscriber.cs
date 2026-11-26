using LogiMarket.Application.Events;
using LogiMarket.Application.Ports.Persistence;
using LogiMarket.Domain.Entities;

namespace LogiMarket.Infrastructure.Events.Subscribers;

public class AuditoriaSubscriber : ISubscriber<PedidoPagadoEvent>
{
    private readonly IRepositorioAuditoria _auditoria;
    public AuditoriaSubscriber(IRepositorioAuditoria auditoria) { _auditoria = auditoria; }

    public async Task HandleAsync(PedidoPagadoEvent evento, CancellationToken ct = default)
    {
        var registro = new Auditoria("SISTEMA", "PedidoPagado",
            $"Pedido {evento.PedidoId:N} pagado por ${evento.Total:0.00}");
        await _auditoria.AgregarAsync(registro, ct);
    }
}