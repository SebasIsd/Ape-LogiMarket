namespace LogiMarket.Application.Ports.Persistence;

public interface IUnitOfWork
{
    Task<int> SaveChangesAsync(CancellationToken ct = default);
    Task<ITransaccion> BeginTransactionAsync(CancellationToken ct = default);
}

public interface ITransaccion : IAsyncDisposable
{
    Task CommitAsync(CancellationToken ct = default);
    Task RollbackAsync(CancellationToken ct = default);
}