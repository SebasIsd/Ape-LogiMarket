namespace LogiMarket.Application.Commands;

public class CommandInvoker : ICommandInvoker
{
    private readonly List<RegistroComando> _historial = new();

    public async Task<TResult> ExecuteAsync<TResult>(ICommand<TResult> command, CancellationToken ct = default)
    {
        try
        {
            var resultado = await command.ExecuteAsync(ct);
            _historial.Add(new RegistroComando(command.GetType().Name, DateTime.UtcNow, true, null));
            return resultado;
        }
        catch (Exception ex)
        {
            _historial.Add(new RegistroComando(command.GetType().Name, DateTime.UtcNow, false, ex.Message));
            throw;
        }
    }

    public Task<IReadOnlyList<RegistroComando>> HistorialAsync(CancellationToken ct = default)
        => Task.FromResult<IReadOnlyList<RegistroComando>>(_historial.AsReadOnly());
}