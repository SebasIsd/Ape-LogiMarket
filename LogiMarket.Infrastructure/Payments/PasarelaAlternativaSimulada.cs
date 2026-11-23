using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LogiMarket.Infrastructure.Payments;
/// <summary>
/// Segunda pasarela simulada alternativa (para demostrar DI sustituible).
/// </summary>
public class PasarelaAlternativaSimulada
{
    public string Cobrar(decimal montoDolares, string referencia)
    {
        if (montoDolares <= 0) throw new ArgumentException("Monto inválido");
        return $"ALT-{Guid.NewGuid():N}"[..20];
    }
}