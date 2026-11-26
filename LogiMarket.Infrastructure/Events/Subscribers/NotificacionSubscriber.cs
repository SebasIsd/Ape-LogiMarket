using LogiMarket.Application.Events;
using LogiMarket.Application.Ports.Notifications;

namespace LogiMarket.Infrastructure.Events.Subscribers;

public class NotificacionSubscriber : ISubscriber<PedidoPagadoEvent>
{
    private readonly INotificadorFactory _notificadorFactory;
    public NotificacionSubscriber(INotificadorFactory notificadorFactory) { _notificadorFactory = notificadorFactory; }

    public async Task HandleAsync(PedidoPagadoEvent evento, CancellationToken ct = default)
    {
        var notificador = _notificadorFactory.Crear("EMAIL");
        await notificador.EnviarAsync(
            "sistema@logimarket.local",
            "Pedido Pagado",
            $"El pedido {evento.PedidoId:N} ha sido pagado por ${evento.Total:0.00}",
            ct);
    }
}