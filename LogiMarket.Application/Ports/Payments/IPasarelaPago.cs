using LogiMarket.Domain.ValueObjects;

namespace LogiMarket.Application.Ports.Payments;

public interface IPasarelaPago
{
    Task<ResultadoPago> CobrarAsync(Money monto, string claveIdempotencia, CancellationToken ct = default);
}

public record ResultadoPago(bool Aprobado, string? Referencia, string? Error);