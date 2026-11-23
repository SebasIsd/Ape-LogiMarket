using LogiMarket.Application.Ports.Payments;
using LogiMarket.Domain.ValueObjects;
using Microsoft.Extensions.Logging;

namespace LogiMarket.Infrastructure.Payments;
//patron aplicado : Decorator, para agregar auditoria a la pasarela de pago sin modificar su implementacion
public class AuditoriaPagoDecorator : IPasarelaPago
{
    private readonly IPasarelaPago _inner;
    private readonly ILogger<AuditoriaPagoDecorator> _log;

    public AuditoriaPagoDecorator(IPasarelaPago inner, ILogger<AuditoriaPagoDecorator> log)
    {
        _inner = inner;
        _log = log;
    }

    public async Task<ResultadoPago> CobrarAsync(Money monto, string clave, CancellationToken ct = default)
    {
        var corr = Guid.NewGuid().ToString("N")[..8];
        _log.LogInformation("[{Corr}] INICIO cobro Monto={Monto} Clave={Clave}", corr, monto.Amount, clave);
        try
        {
            var r = await _inner.CobrarAsync(monto, clave, ct);
            if (r.Aprobado)
                _log.LogInformation("[{Corr}] FIN cobro OK Ref={Ref}", corr, r.Referencia);
            else
                _log.LogWarning("[{Corr}] FIN cobro RECHAZADO Error={Err}", corr, r.Error);
            return r;
        }
        catch (Exception ex)
        {
            _log.LogError(ex, "[{Corr}] ERROR cobro", corr);
            throw;
        }
    }
}