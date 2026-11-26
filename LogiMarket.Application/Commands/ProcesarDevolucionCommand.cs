using LogiMarket.Application.DTOs;
using LogiMarket.Application.Ports.Persistence;
using LogiMarket.Application.UseCases.Devoluciones;
using LogiMarket.Domain.Enums;
using LogiMarket.Domain.Exceptions;

namespace LogiMarket.Application.Commands;

public class ProcesarDevolucionCommand : ICommand<ResultadoOperacion>
{
    private readonly Guid _pedidoId;
    private readonly MotivoDevolucion _motivo;
    private readonly IRepositorioPedido _repo;
    private readonly ProcesarDevolucionTemplate _handler;

    public ProcesarDevolucionCommand(Guid pedidoId, MotivoDevolucion motivo,
        IRepositorioPedido repo, ProcesarDevolucionTemplate handler)
    {
        _pedidoId = pedidoId;
        _motivo = motivo;
        _repo = repo;
        _handler = handler;
    }

    public async Task<ResultadoOperacion> ExecuteAsync(CancellationToken ct = default)
    {
        var pedido = await _repo.ObtenerPorIdAsync(_pedidoId, ct)
            ?? throw new DomainException("Pedido no encontrado");

        var devolucion = await _handler.EjecutarAsync(pedido, _motivo, ct);

        if (devolucion.Aceptada)
            return ResultadoOperacion.Ok($"Devolución aceptada. Reembolso: ${devolucion.Reembolso.Amount:0.00}");
        else
            return ResultadoOperacion.Error("Devolución rechazada según la política");
    }
}