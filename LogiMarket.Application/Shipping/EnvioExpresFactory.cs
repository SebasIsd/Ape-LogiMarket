namespace LogiMarket.Application.Shipping;

/// <summary>
/// Fábrica concreta que SOLO produce la familia EXPRÉS.
/// Imposible que mezcle variantes ESTÁNDAR por diseño.
/// </summary>
public class EnvioExpresFactory : IEnvioAbstractFactory
{
    public IFamiliaEnvio CrearFamilia() => new EnvioExpres();
}