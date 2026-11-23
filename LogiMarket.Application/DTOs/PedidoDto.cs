namespace LogiMarket.Application.DTOs;

public record PedidoDto(
    Guid Id,
    string ClienteNombre,
    string Estado,
    decimal Subtotal,
    decimal Descuento,
    decimal Tarifa,
    decimal Total,
    string Estrategia,
    DateTime FechaCreacion);
