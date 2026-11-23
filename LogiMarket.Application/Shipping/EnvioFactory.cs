using LogiMarket.Domain.Enums;
using LogiMarket.Domain.Exceptions;

namespace LogiMarket.Application.Shipping;

public class EnvioFactory : IEnvioAbstractFactory
{
    public IFamiliaEnvio Crear(TipoEnvio tipo) => tipo switch
    {
        TipoEnvio.Estandar => new EnvioEstandar(),
        TipoEnvio.Expres => new EnvioExpres(),
        _ => throw new DomainException($"Tipo de envío no soportado: {tipo}")
    };
}