using LogiMarket.Application.DTOs;
using LogiMarket.Application.Facades;

namespace LogiMarket.Application.Commands;

public class ConfirmarPedidoCommand : ICommand<ResultadoOperacion>
{
    private readonly Guid _pedidoId;
    private readonly string _claveIdempotencia;
    private readonly ConfirmacionPedidoFacade _facade;

    public ConfirmarPedidoCommand(Guid pedidoId, string claveIdempotencia, ConfirmacionPedidoFacade facade)
    {
        _pedidoId = pedidoId;
        _claveIdempotencia = claveIdempotencia;
        _facade = facade;
    }

    public async Task<ResultadoOperacion> ExecuteAsync(CancellationToken ct = default)
    {
        return await _facade.ConfirmarAsync(_pedidoId, _claveIdempotencia, ct);
    }
}