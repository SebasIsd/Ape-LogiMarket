namespace LogiMarket.Infrastructure.Payments;

/// <summary>
/// Pasarela "legada" con firma incompatible: recibe centavos (long) y una referencia.
/// </summary>
public class PasarelaLegadaSimulada
{
    private readonly bool _siempreAprobado;
    public PasarelaLegadaSimulada(bool siempreAprobado = true) { _siempreAprobado = siempreAprobado; }

    public long CobrarCentavos(long centavos, string referencia)
    {
        if (centavos <= 0) throw new ArgumentException("Monto inválido");
        if (!_siempreAprobado) throw new InvalidOperationException("Cobro rechazado por la pasarela");
        return centavos; // "aprobado"
    }
}