using LogiMarket.Domain.Entities;

namespace LogiMarket.WinForms.Interpreter;
public class ExpresionOr : ExpresionFiltro
{
    private readonly ExpresionFiltro _izq;
    private readonly ExpresionFiltro _der;
    public ExpresionOr(ExpresionFiltro izq, ExpresionFiltro der) { _izq = izq; _der = der; }
    public override bool Evaluar(Pedido p) => _izq.Evaluar(p) || _der.Evaluar(p);
}