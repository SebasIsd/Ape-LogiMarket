using LogiMarket.Application.Commands;
using LogiMarket.Application.DTOs;
using LogiMarket.Application.Ports.Persistence;
using LogiMarket.Application.Commands;
using LogiMarket.Application.DTOs;
using LogiMarket.Application.Ports.Persistence;

namespace LogiMarket.Application.Queries;

public class ObtenerPedidosQuery : IQuery<IReadOnlyList<PedidoDto>>
{
    private readonly IRepositorioPedido _repo;

    public ObtenerPedidosQuery(IRepositorioPedido repo)
    {
        _repo = repo;
    }

    public async Task<IReadOnlyList<PedidoDto>> ExecuteAsync(CancellationToken ct = default)
    {
        var pedidos = await _repo.ListarAsync(ct);
        return pedidos.Select(p => new PedidoDto(
            p.Id,
            p.Cliente?.Nombre ?? "—",
            p.EstadoActual,
            p.Subtotal.Amount,
            p.Descuento.Amount,
            p.TarifaEnvio.Amount,
            p.Total.Amount,
            p.EstrategiaDescuentoAplicada,
            p.FechaCreacion
        )).ToList().AsReadOnly();
    }
}