using LogiMarket.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LogiMarket.WinForms.Interpreter;
public class ExpresionIgualdad : ExpresionFiltro
{
    private readonly string _campo;
    private readonly string _valor;
    public ExpresionIgualdad(string campo, string valor) { _campo = campo; _valor = valor; }

    public override bool Evaluar(Pedido p)
    {
        return _campo.ToUpper() switch
        {
            "ESTADO" => p.EstadoActual.Equals(_valor, StringComparison.OrdinalIgnoreCase),
            "CLIENTE" => (p.Cliente?.Nombre ?? "").Contains(_valor, StringComparison.OrdinalIgnoreCase),
            _ => false
        };
    }
}