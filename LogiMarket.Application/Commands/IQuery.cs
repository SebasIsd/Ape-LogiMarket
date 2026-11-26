namespace LogiMarket.Application.Commands;

public interface IQuery<TResult>
{
    Task<TResult> ExecuteAsync(CancellationToken ct = default);
}