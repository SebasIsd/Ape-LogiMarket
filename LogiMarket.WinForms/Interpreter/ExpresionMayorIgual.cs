using LogiMarket.Domain.Entities;

namespace LogiMarket.WinForms.Interpreter;
public class ExpresionMayorIgual : ExpresionFiltro
{
    private readonly string _campo;
    private readonly decimal _valor;
    public ExpresionMayorIgual(string campo, decimal valor) { _campo = campo; _valor = valor; }

    public override bool Evaluar(Pedido p)
    {
        return _campo.ToUpper() switch
        {
            "TOTAL" => p.Total.Amount >= _valor,
            "SUBTOTAL" => p.Subtotal.Amount >= _valor,
            _ => false
        };
    }
}