using LogiMarket.Domain.Enums;
using LogiMarket.Domain.ValueObjects;

namespace LogiMarket.Application.Discount;

public class Vip10Strategy : IDescuentoStrategy
{
    public string Nombre => "VIP";
    public Money CalcularDescuento(Money subtotal, int unidades, CategoriaCliente categoria)
        => categoria == CategoriaCliente.Vip ? subtotal.Multiply(0.10m) : Money.Zero;
}