using LogiMarket.Domain.Enums;
using LogiMarket.Domain.ValueObjects;

namespace LogiMarket.Application.Discount;

public interface IDescuentoStrategy
{
    string Nombre { get; }
    Money CalcularDescuento(Money subtotal, int unidadesFisicasFinales, CategoriaCliente categoria);
}