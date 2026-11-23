using LogiMarket.Domain.Enums;
using LogiMarket.Domain.ValueObjects;

namespace LogiMarket.Application.Discount;

public class SinDescuentoStrategy : IDescuentoStrategy
{
    public string Nombre => "NORMAL";
    public Money CalcularDescuento(Money subtotal, int unidades, CategoriaCliente categoria) => Money.Zero;
}