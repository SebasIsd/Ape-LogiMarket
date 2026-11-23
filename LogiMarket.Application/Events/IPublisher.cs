namespace LogiMarket.Application.Events;

public interface IPublisher
{
    Task PublishAsync(IDominioEvento evento, CancellationToken ct = default);
}

public interface ISubscriber<T> where T : IDominioEvento
{
    Task HandleAsync(T evento, CancellationToken ct = default);
}