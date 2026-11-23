using LogiMarket.Domain.Enums;
using LogiMarket.Domain.ValueObjects;

namespace LogiMarket.Application.Discount;

public class Mayorista15Strategy : IDescuentoStrategy
{
    public string Nombre => "MAYORISTA";
    public Money CalcularDescuento(Money subtotal, int unidades, CategoriaCliente categoria)
        => unidades >= 10 ? subtotal.Multiply(0.15m) : Money.Zero;
}