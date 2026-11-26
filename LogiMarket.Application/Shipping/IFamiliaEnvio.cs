using LogiMarket.Domain.ValueObjects;

namespace LogiMarket.Application.Shipping;

/// <summary>
/// 1. LA FAMILIA DE PRODUCTOS: Define los métodos que toda familia de envío debe implementar.
/// </summary>
public interface IFamiliaEnvio
{
    string Nombre { get; }
    Money CalcularTarifa();
    int PlazoDias { get; }
    string GenerarGuia();
    string GenerarEtiqueta(string destino);
}