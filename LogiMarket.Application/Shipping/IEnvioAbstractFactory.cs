using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LogiMarket.Application.Shipping;
/// <summary>
/// 2. LA FÁBRICA ABSTRACTA: Define el contrato para crear una familia de envío.
/// </summary>
public interface IEnvioAbstractFactory
{
    IFamiliaEnvio CrearFamilia();
}