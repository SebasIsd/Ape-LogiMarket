using LogiMarket.Domain.Enums;
using LogiMarket.Domain.Exceptions;

namespace LogiMarket.Application.Shipping;

/// <summary>
/// Proveedor que resuelve el TipoEnvio a la Fábrica Abstracta concreta correspondiente.
/// </summary>
public class EnvioFactoryProvider
{
    public IEnvioAbstractFactory ObtenerFactory(TipoEnvio tipo) => tipo switch
    {
        TipoEnvio.Estandar => new EnvioEstandarFactory(),
        TipoEnvio.Expres => new EnvioExpresFactory(),
        _ => throw new DomainException($"Tipo de envío no soportado: {tipo}")
    };
}