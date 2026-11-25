using LogiMarket.Domain.ValueObjects;
using Xunit;

namespace LogiMarket.Tests.Domain;

public class MoneyTests
{
    [Fact]
    public void Multiply_Redondea_AwayFromZero()
    {
        var money = new Money(100m);
        var resultado = money.Multiply(0.15m);
        Assert.Equal(15.00m, resultado.Amount);
    }

    [Fact]
    public void Add_Suma_Correctamente()
    {
        var a = new Money(50m);
        var b = new Money(9.00m);
        Assert.Equal(59.00m, a.Add(b).Amount);
    }

    [Fact]
    public void Subtract_Resta_Correctamente()
    {
        var a = new Money(60m);
        var b = new Money(6m);
        Assert.Equal(54.00m, a.Subtract(b).Amount);
    }

    [Fact]
    public void Zero_Es_Cero()
    {
        Assert.Equal(0m, Money.Zero.Amount);
    }
}