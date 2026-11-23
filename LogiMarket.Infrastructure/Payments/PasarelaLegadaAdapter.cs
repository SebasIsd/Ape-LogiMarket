using LogiMarket.Application.Ports.Payments;
using LogiMarket.Domain.ValueObjects;

namespace LogiMarket.Infrastructure.Payments;

public class PasarelaLegadaAdapter : IPasarelaPago
{
    private readonly PasarelaLegadaSimulada _legado;
    public PasarelaLegadaAdapter(PasarelaLegadaSimulada legado) { _legado = legado; }

    public Task<ResultadoPago> CobrarAsync(Money monto, string claveIdempotencia, CancellationToken ct = default)
    {
        try
        {
            var centavos = (long)Math.Round(monto.Amount * 100m, 0, MidpointRounding.AwayFromZero);
            var refLegada = _legado.CobrarCentavos(centavos, claveIdempotencia);
            return Task.FromResult(new ResultadoPago(true, $"LEG-{refLegada}", null));
        }
        catch (Exception ex)
        {
            return Task.FromResult(new ResultadoPago(false, null, ex.Message));
        }
    }
}