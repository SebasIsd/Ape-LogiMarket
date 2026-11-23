using LogiMarket.Application.Shipping;
using LogiMarket.Domain.ValueObjects;
namespace LogiMarket.Application.Shipping;
public class EnvioExpres : IFamiliaEnvio
{
    public string Nombre => "EXPRÉS";
    public Money CalcularTarifa() => new(12.00m);
    public int PlazoDias => 1;
    public string GenerarGuia() => $"EXP-{DateTime.UtcNow:yyyyMMddHHmmss}-{Guid.NewGuid().ToString("N")[..6].ToUpper()}";
    public string GenerarEtiqueta(string destino) => $"[EXPRÉS] 1 día — {destino} — Guía {GenerarGuia()}";
}