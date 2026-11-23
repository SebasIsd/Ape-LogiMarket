using LogiMarket.Domain.Enums;
using LogiMarket.Domain.ValueObjects;

namespace LogiMarket.Application.Discount;

/// <summary>
/// Contexto Strategy: selecciona la política con mayor prioridad que aplique.
/// Orden: Mayorista > VIP > Normal. OCP: agregar nuevas políticas sin editar esta clase.
/// </summary>
public class CalculadoraDescuentoContext
{
    private readonly IReadOnlyList<IDescuentoStrategy> _estrategias;

    public CalculadoraDescuentoContext(IEnumerable<IDescuentoStrategy> estrategias)
    {
        // Orden explícito de prioridad
        _estrategias = estrategias
            .OrderByDescending(Prioridad)
            .ToList();
    }

    private static int Prioridad(IDescuentoStrategy e) => e switch
    {
        Mayorista15Strategy => 3,
        Vip10Strategy => 2,
        SinDescuentoStrategy => 0,
        _ => 1
    };

    public (IDescuentoStrategy Aplicada, Money Descuento) Calcular(
        Money subtotal, int unidadesFisicasFinales, CategoriaCliente categoria)
    {
        foreach (var e in _estrategias)
        {
            var d = e.CalcularDescuento(subtotal, unidadesFisicasFinales, categoria);
            if (d.Amount > 0) return (e, d);
        }
        return (new SinDescuentoStrategy(), Money.Zero);
    }
}