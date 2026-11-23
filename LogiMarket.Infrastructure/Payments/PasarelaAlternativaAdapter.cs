using LogiMarket.Application.Ports.Payments;
using LogiMarket.Domain.ValueObjects;

namespace LogiMarket.Infrastructure.Payments;
public class PasarelaAlternativaAdapter : IPasarelaPago
{
    private readonly PasarelaAlternativaSimulada _alt;
    public PasarelaAlternativaAdapter(PasarelaAlternativaSimulada alt) { _alt = alt; }

    public Task<ResultadoPago> CobrarAsync(Money monto, string claveIdempotencia, CancellationToken ct = default)
    {
        try
        {
            var refAlt = _alt.Cobrar(monto.Amount, claveIdempotencia);
            return Task.FromResult(new ResultadoPago(true, refAlt, null));
        }
        catch (Exception ex)
        {
            return Task.FromResult(new ResultadoPago(false, null, ex.Message));
        }
    }
}
