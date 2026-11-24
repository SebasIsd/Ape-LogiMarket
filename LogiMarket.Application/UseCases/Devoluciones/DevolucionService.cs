using LogiMarket.Application.Ports.Payments;
using LogiMarket.Application.Ports.Persistence;
using LogiMarket.Domain.Entities;
using LogiMarket.Domain.Entities.Catalog;
using LogiMarket.Domain.Enums;
using LogiMarket.Domain.Exceptions;

namespace LogiMarket.Application.UseCases.Devoluciones;

/// <summary>
/// Caso de uso: procesa una devolución de punta a punta (Template Method + persistencia real).
/// Reintegra stock, registra movimientos de entrada, guarda la devolución y cambia el estado del pedido.
/// </summary>
public class DevolucionService
{
    private readonly IRepositorioPedido _pedidos;
    private readonly IRepositorioCatalogo _catalogo;
    private readonly IRepositorioMovimientos _movimientos;
    private readonly IRepositorioAuditoria _auditoria;
    private readonly IUnitOfWork _uow;
    private readonly ISesionUsuario _sesion;

    public DevolucionService(
        IRepositorioPedido pedidos,
        IRepositorioCatalogo catalogo,
        IRepositorioMovimientos movimientos,
        IRepositorioAuditoria auditoria,
        IUnitOfWork uow,
        ISesionUsuario sesion)
    {
        _pedidos = pedidos;
        _catalogo = catalogo;
        _movimientos = movimientos;
        _auditoria = auditoria;
        _uow = uow;
        _sesion = sesion;
    }

    public async Task<Devolucion> ProcesarAsync(Guid pedidoId, MotivoDevolucion motivo, CancellationToken ct = default)
    {
        var pedido = await _pedidos.ObtenerPorIdAsync(pedidoId, ct)
            ?? throw new DomainException("Pedido no encontrado.");

        Func<Pedido, CancellationToken, Task> reintegrar = ReintegrarStockAsync;
        Func<Devolucion, CancellationToken, Task> persistir = async (dev, c) =>
        {
            pedido.RegistrarDevolucion(dev);          // ENTREGADO -> CANCELADO si fue aceptada
            await _pedidos.ActualizarAsync(pedido, c);
        };

        ProcesarDevolucionTemplate handler = motivo switch
        {
            MotivoDevolucion.Defecto => new DevolucionDefectoHandler(reintegrar, persistir),
            MotivoDevolucion.Arrepentimiento => new DevolucionArrepentimientoHandler(reintegrar, persistir),
            _ => throw new DomainException("Motivo de devolución inválido.")
        };

        await using var tx = await _uow.BeginTransactionAsync(ct);
        try
        {
            var devolucion = await handler.EjecutarAsync(pedido, motivo, ct);
            await tx.CommitAsync(ct);

            await RegistrarAuditoriaAsync(
                devolucion.Aceptada ? "DEVOLUCION_ACEPTADA" : "DEVOLUCION_RECHAZADA",
                $"Pedido {pedido.Id.ToString("N")[..8].ToUpperInvariant()} · {motivo} · reembolso $ {devolucion.Reembolso.Amount:0.00}",
                ct);
            return devolucion;
        }
        catch
        {
            await tx.RollbackAsync(ct);
            throw;
        }
    }

    private async Task ReintegrarStockAsync(Pedido pedido, CancellationToken ct)
    {
        var porSku = new Dictionary<string, int>();
        foreach (var detalle in pedido.Detalles)
        {
            var elemento = await _catalogo.ObtenerPorSkuAsync(detalle.Sku, ct)
                ?? throw new DomainException($"SKU no encontrado: {detalle.Sku}");
            foreach (var (producto, cantidad) in elemento.ExpandirSkusFinales(detalle.Cantidad))
            {
                porSku.TryGetValue(producto.Sku, out var acumulado);
                porSku[producto.Sku] = acumulado + cantidad;
            }
        }

        var referencia = pedido.Id.ToString("N")[..8].ToUpperInvariant();
        foreach (var (sku, cantidad) in porSku)
        {
            var producto = await _catalogo.ObtenerPorSkuAsync(sku, ct) as ProductoSimple
                ?? throw new DomainException($"SKU final no es producto simple: {sku}");

            producto.ReintegrarStock(cantidad);
            await _catalogo.ActualizarProductoAsync(producto, ct);
            await _movimientos.AgregarAsync(
                new MovimientoInventario(sku, cantidad, TipoMovimientoInventario.Entrada, "Devolución aceptada", referencia), ct);
        }
    }

    private async Task RegistrarAuditoriaAsync(string accion, string detalle, CancellationToken ct)
    {
        try
        {
            var usuario = string.IsNullOrEmpty(_sesion.Username) ? "sistema" : _sesion.Username;
            await _auditoria.AgregarAsync(new Auditoria(usuario, accion, detalle), ct);
        }
        catch { /* la auditoría nunca debe romper la operación principal */ }
    }
}
