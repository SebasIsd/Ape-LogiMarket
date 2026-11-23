using LogiMarket.Domain.Entities;
using LogiMarket.Domain.Enums;
using LogiMarket.Domain.Exceptions;
using LogiMarket.Domain.ValueObjects;

namespace LogiMarket.Application.UseCases.Devoluciones;

/// <summary>
/// Template Method: secuencia invariante con pasos variables según el motivo.
/// </summary>
public abstract class ProcesarDevolucionTemplate
{
    public async Task<Devolucion> EjecutarAsync(Pedido pedido, MotivoDevolucion motivo, CancellationToken ct)
    {
        VerificarElegibilidad(pedido);
        var diasTranscurridos = CalcularDiasTranscurridos(pedido);
        var aceptada = EvaluarCondicion(diasTranscurridos);
        var reembolso = CalcularReembolso(pedido, aceptada);

        if (aceptada)
        {
            ReintegrarStock(pedido);
        }

        var dev = new Devolucion(pedido.Id, motivo, reembolso, aceptada);
        await PersistirAsync(dev, ct);
        return dev;
    }

    protected void VerificarElegibilidad(Pedido pedido)
    {
        if (pedido.EstadoActual != "ENTREGADO")
            throw new OperacionNoPermitidaException("Solo se devuelven pedidos ENTREGADO.");
        if (pedido.Devolucion is not null)
            throw new OperacionNoPermitidaException("El pedido ya fue devuelto.");
    }

    protected int CalcularDiasTranscurridos(Pedido pedido)
        => (int)(DateTime.UtcNow - (pedido.FechaEntrega ?? DateTime.UtcNow)).TotalDays;

    protected abstract bool EvaluarCondicion(int diasTranscurridos);

    protected Money CalcularReembolso(Pedido pedido, bool aceptada)
    {
        if (!aceptada) return Money.Zero;
        // Reembolso = subtotal − descuento (NO incluye tarifa de envío)
        return pedido.Subtotal.Subtract(pedido.Descuento);
    }

    protected abstract void ReintegrarStock(Pedido pedido);
    protected abstract Task PersistirAsync(Devolucion dev, CancellationToken ct);
}