using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LogiMarket.Application.Validation;
public record ResultadoValidacion(bool Ok, string? Error)
{
    public static ResultadoValidacion Exito => new(true, null);
    public static ResultadoValidacion Falla(string error) => new(false, error);
}