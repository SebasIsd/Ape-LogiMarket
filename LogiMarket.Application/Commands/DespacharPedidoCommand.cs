using LogiMarket.Application.DTOs;
using LogiMarket.Application.Ports.Persistence;
using LogiMarket.Application.Shipping;
using LogiMarket.Domain.Entities;
using LogiMarket.Domain.Enums;
using LogiMarket.Domain.Exceptions;

namespace LogiMarket.Application.Commands;

public class DespacharPedidoCommand : ICommand<ResultadoOperacion>
{
    private readonly Guid _pedidoId;
    private readonly TipoEnvio _tipoEnvio;
    private readonly IRepositorioPedido _repo;
    private readonly IEnvioAbstractFactory _envioFactory;

    public DespacharPedidoCommand(Guid pedidoId, TipoEnvio tipoEnvio,
        IRepositorioPedido repo, IEnvioAbstractFactory envioFactory)
    {
        _pedidoId = pedidoId;
        _tipoEnvio = tipoEnvio;
        _repo = repo;
        _envioFactory = envioFactory;
    }

    public async Task<ResultadoOperacion> ExecuteAsync(CancellationToken ct = default)
    {
        var pedido = await _repo.ObtenerPorIdAsync(_pedidoId, ct)
            ?? throw new DomainException("Pedido no encontrado");

        var familia = _envioFactory.Crear(_tipoEnvio);
        var despacho = new Despacho(pedido.Id, familia.GenerarGuia(), familia.Nombre, familia.PlazoDias);

        pedido.Despachar(despacho);
        await _repo.ActualizarAsync(pedido, ct);

        return ResultadoOperacion.Ok($"Pedido despachado con guía {despacho.Guia}");
    }
}