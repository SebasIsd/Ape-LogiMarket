using LogiMarket.Domain.ValueObjects;

namespace LogiMarket.Application.Shipping;

public class EnvioEstandar : IFamiliaEnvio
{
    public string Nombre => "ESTÁNDAR";
    public Money CalcularTarifa() => new(5.00m);
    public int PlazoDias => 3;
    public string GenerarGuia() => $"STD-{DateTime.UtcNow:yyyyMMddHHmmss}-{Guid.NewGuid().ToString("N")[..6].ToUpper()}";
    public string GenerarEtiqueta(string destino) => $"[ESTÁNDAR] 3 días — {destino} — Guía {GenerarGuia()}";
}