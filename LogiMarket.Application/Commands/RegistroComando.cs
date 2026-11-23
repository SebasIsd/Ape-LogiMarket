using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LogiMarket.Application.Commands;
public record RegistroComando(string NombreComando, DateTime EjecutadoEn, bool Exito, string? Error);
