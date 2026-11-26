namespace LogiMarket.Application.Shipping;

/// <summary>
/// Fábrica concreta que SOLO produce la familia ESTÁNDAR.
/// Imposible que mezcle variantes EXPRÉS por diseño.
/// </summary>
public class EnvioEstandarFactory : IEnvioAbstractFactory
{
    public IFamiliaEnvio CrearFamilia() => new EnvioEstandar();
}