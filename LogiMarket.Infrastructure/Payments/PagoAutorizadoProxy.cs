using LogiMarket.Application.Ports.Payments;
using LogiMarket.Domain.Enums;
using LogiMarket.Domain.ValueObjects;
using Microsoft.Extensions.Logging;

namespace LogiMarket.Infrastructure.Payments;

public class PagoAutorizadoProxy : IPasarelaPago
{
    private readonly IPasarelaPago _inner;
    private readonly ISesionUsuario _sesion;
    private readonly ILogger<PagoAutorizadoProxy> _log;

    public PagoAutorizadoProxy(IPasarelaPago inner, ISesionUsuario sesion, ILogger<PagoAutorizadoProxy> log)
    {
        _inner = inner;
        _sesion = sesion;
        _log = log;
    }

    public Task<ResultadoPago> CobrarAsync(Money monto, string clave, CancellationToken ct = default)
    {
        if (_sesion.Rol != RolUsuario.Admin && _sesion.Rol != RolUsuario.Ventas)
        {
            _log.LogWarning("Cobro DENEGADO para rol {Rol} (usuario {User})", _sesion.Rol, _sesion.Username);
            return Task.FromResult(new ResultadoPago(false, null, $"Rol {_sesion.Rol} no autorizado para cobrar."));
        }
        return _inner.CobrarAsync(monto, clave, ct);
    }
}