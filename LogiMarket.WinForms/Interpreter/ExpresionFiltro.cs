using LogiMarket.Domain.Entities;

namespace LogiMarket.WinForms.Interpreter;
/// <summary>
/// Interpreter: parser de filtros de consulta con gramática acotada.
/// Soporta: ESTADO = valor, TOTAL >= número, CLIENTE = texto, AND, OR, paréntesis.
/// </summary>
public abstract class ExpresionFiltro
{
    public abstract bool Evaluar(Pedido pedido);
}