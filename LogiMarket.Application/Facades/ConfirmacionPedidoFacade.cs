using LogiMarket.Application.Commands;
using LogiMarket.Application.DTOs;
using LogiMarket.Application.Events;
using LogiMarket.Application.Ports.Payments;
using LogiMarket.Application.Ports.Persistence;
using LogiMarket.Application.Validation;
using LogiMarket.Domain.Entities;
using LogiMarket.Domain.Entities.Catalog;
using LogiMarket.Domain.Enums;
using LogiMarket.Domain.Exceptions;

namespace LogiMarket.Application.Facades;

public class ConfirmacionPedidoFacade
{
    private readonly PipelineValidacionPedido _pipeline;
    private readonly IPasarelaPago _pasarela;
    private readonly IRepositorioPedido _repoPedido;
    private readonly IRepositorioCatalogo _catalogo;
    private readonly IRepositorioMovimientos _movimientos;
    private readonly IUnitOfWork _uow;
    private readonly IRepositorioOutbox _outbox;
    private readonly IPublisher _publisher;
    private readonly ISesionUsuario _sesion;

    public ConfirmacionPedidoFacade(
        PipelineValidacionPedido pipeline,
        IPasarelaPago pasarela,
        IRepositorioPedido repoPedido,
        IRepositorioCatalogo catalogo,
        IRepositorioMovimientos movimientos,
        IUnitOfWork uow,
        IRepositorioOutbox outbox,
        IPublisher publisher,
        ISesionUsuario sesion)
    {
        _pipeline = pipeline;
        _pasarela = pasarela;
        _repoPedido = repoPedido;
        _catalogo = catalogo;
        _movimientos = movimientos;
        _uow = uow;
        _outbox = outbox;
        _publisher = publisher;
        _sesion = sesion;
    }

    public async Task<ResultadoOperacion> ConfirmarAsync(Guid pedidoId, string claveIdempotencia, CancellationToken ct = default)
    {
        // 1. Autorización por rol
        if (_sesion.Rol != RolUsuario.Admin && _sesion.Rol != RolUsuario.Ventas)
            return ResultadoOperacion.Error($"Rol {_sesion.Rol} no autorizado para cobrar.");

        // 2. Idempotencia
        if (await _repoPedido.ExistePagoPorClaveAsync(claveIdempotencia, ct))
            return ResultadoOperacion.Error("Ya existe un pago con esa clave de idempotencia.");

        var pedido = await _repoPedido.ObtenerPorIdAsync(pedidoId, ct)
            ?? throw new DomainException("Pedido no encontrado.");

        // 3. Pipeline de validación (Chain of Responsibility)
        var validacion = await _pipeline.EjecutarAsync(pedido, ct);
        if (!validacion.Ok) return ResultadoOperacion.Error(validacion.Error!);

        // 4. Transacción atómica
        await using var tx = await _uow.BeginTransactionAsync(ct);
        try
        {
            // Transición de estado: Borrador → Validado
            pedido.Validar();

            // 5. Cobro simulado a través de la cadena Proxy → Decorator → Adapter
            var resultado = await _pasarela.CobrarAsync(pedido.Total, claveIdempotencia, ct);
            if (!resultado.Aprobado)
            {
                await tx.RollbackAsync(ct);
                return ResultadoOperacion.Error(resultado.Error ?? "Cobro rechazado.");
            }

            // 6. Registrar pago en el agregado
            var pago = new Pago(
                pedido.Id,
                pedido.Total,
                resultado.Referencia!,
                claveIdempotencia,
                "PasarelaSimulada");
            pedido.Pagar(pago);

            // 7. Descontar stock UNA SOLA VEZ, expandiendo kits hasta SKUs finales
            await DescontarStockDeProductosFinalesAsync(pedido, claveIdempotencia, ct);

            // 8. Persistir pedido actualizado
            await _repoPedido.ActualizarAsync(pedido, ct);

            // 9. Evento Outbox (misma transacción)
            var payload = System.Text.Json.JsonSerializer.Serialize(new
            {
                pedido.Id,
                pedido.Total.Amount,
                ClaveIdempotencia = claveIdempotencia
            });
            await _outbox.AgregarAsync(new EventoOutbox("PedidoPagado", payload), ct);

            await tx.CommitAsync(ct);

            // 10. Publicar evento de dominio (después del commit)
            await _publisher.PublishAsync(
                new PedidoPagadoEvent(Guid.NewGuid(), DateTime.UtcNow, "PedidoPagado", pedido.Id, pedido.Total.Amount),
                ct);

            return ResultadoOperacion.Ok(new { pedido.Id, pedido.Total.Amount, pago.Referencia });
        }
        catch
        {
            await tx.RollbackAsync(ct);
            throw;
        }
    }

    /// <summary>
    /// Expande cada línea del pedido hasta los SKU finales (productos simples)
    /// y descuenta el stock de cada uno. Registra un movimiento de inventario por SKU.
    /// </summary>
    private async Task DescontarStockDeProductosFinalesAsync(Pedido pedido, string referencia, CancellationToken ct)
    {
        // Agrupa las cantidades requeridas por SKU final
        var requeridoPorSku = new Dictionary<string, int>(); 

        foreach (var detalle in pedido.Detalles)
        {
            var elemento = await _catalogo.ObtenerPorSkuAsync(detalle.Sku, ct)
                ?? throw new DomainException($"SKU no encontrado: {detalle.Sku}");

            foreach (var (producto, cantidad) in elemento.ExpandirSkusFinales(detalle.Cantidad))
            {
                requeridoPorSku.TryGetValue(producto.Sku, out var acumulado);
                requeridoPorSku[producto.Sku] = acumulado + cantidad;
            }
        }

        // Descuenta stock y registra movimiento por cada SKU final
        foreach (var (sku, cantidad) in requeridoPorSku)
        {
            var producto = await _catalogo.ObtenerPorSkuAsync(sku, ct) as ProductoSimple
                ?? throw new DomainException($"SKU final no es producto simple: {sku}");

            producto.DescontarStock(cantidad);
            await _catalogo.ActualizarProductoAsync(producto, ct);

            await _movimientos.AgregarAsync(
                new MovimientoInventario(sku, cantidad, TipoMovimientoInventario.Salida, "Venta confirmada", referencia),
                ct);
        }
    }
}