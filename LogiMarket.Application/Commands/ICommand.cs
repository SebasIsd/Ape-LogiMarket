namespace LogiMarket.Application.Commands;

public interface ICommand<TResult>
{
    Task<TResult> ExecuteAsync(CancellationToken ct = default);
}