using LogiMarket.Domain.Entities;

namespace LogiMarket.WinForms.Interpreter;
public class ExpresionAnd : ExpresionFiltro
{
    private readonly ExpresionFiltro _izq;
    private readonly ExpresionFiltro _der;
    public ExpresionAnd(ExpresionFiltro izq, ExpresionFiltro der) { _izq = izq; _der = der; }
    public override bool Evaluar(Pedido p) => _izq.Evaluar(p) && _der.Evaluar(p);
}