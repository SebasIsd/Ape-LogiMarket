using LogiMarket.Domain.ValueObjects;

namespace LogiMarket.Application.Shipping;

public interface IFamiliaEnvio
{
    string Nombre { get; }
    Money CalcularTarifa();
    int PlazoDias { get; }
    string GenerarGuia();
    string GenerarEtiqueta(string destino);
}

public interface IEnvioAbstractFactory
{
    IFamiliaEnvio Crear(Domain.Enums.TipoEnvio tipo);
}