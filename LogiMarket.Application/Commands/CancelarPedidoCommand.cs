using LogiMarket.Application.DTOs;
using LogiMarket.Application.Ports.Persistence;
using LogiMarket.Domain.Exceptions;

namespace LogiMarket.Application.Commands;

public class CancelarPedidoCommand : ICommand<ResultadoOperacion>
{
    private readonly Guid _pedidoId;
    private readonly IRepositorioPedido _repo;

    public CancelarPedidoCommand(Guid pedidoId, IRepositorioPedido repo)
    {
        _pedidoId = pedidoId;
        _repo = repo;
    }

    public async Task<ResultadoOperacion> ExecuteAsync(CancellationToken ct = default)
    {
        var pedido = await _repo.ObtenerPorIdAsync(_pedidoId, ct)
            ?? throw new DomainException("Pedido no encontrado");

        pedido.Cancelar();
        await _repo.ActualizarAsync(pedido, ct);

        return ResultadoOperacion.Ok($"Pedido {_pedidoId:N} cancelado");
    }
}