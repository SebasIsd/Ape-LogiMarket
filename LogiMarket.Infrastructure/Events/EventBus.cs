using LogiMarket.Application.Events;
using Microsoft.Extensions.DependencyInjection;

namespace LogiMarket.Infrastructure.Events;

public class EventBus : IPublisher
{
    private readonly IServiceProvider _serviceProvider;

    public EventBus(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }

    public async Task PublishAsync(IDominioEvento evento, CancellationToken ct = default)
    {
        var eventType = evento.GetType();
        var subscriberType = typeof(ISubscriber<>).MakeGenericType(eventType);

        // Obtiene todos los suscriptores registrados en el contenedor DI
        var subscribers = _serviceProvider.GetServices(subscriberType);

        foreach (var subscriber in subscribers)
        {
            var handleMethod = subscriberType.GetMethod("HandleAsync");
            if (handleMethod != null)
            {
                await (Task)handleMethod.Invoke(subscriber, new object[] { evento, ct });
            }
        }
    }
}