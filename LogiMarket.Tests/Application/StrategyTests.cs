using LogiMarket.Application.Discount;
using LogiMarket.Domain.Enums;
using LogiMarket.Domain.ValueObjects;
using Xunit;

namespace LogiMarket.Tests.Application;

public class StrategyTests
{
    private readonly CalculadoraDescuentoContext _calc;

    public StrategyTests()
    {
        _calc = new CalculadoraDescuentoContext(new IDescuentoStrategy[]
        {
            new SinDescuentoStrategy(),
            new Vip10Strategy(),
            new Mayorista15Strategy()
        });
    }

    [Fact]
    public void Ana_VIP_KIT_TELETRABAJO_Descuento_6()
    {
        var (estrategia, descuento) = _calc.Calcular(new Money(60m), 3, CategoriaCliente.Vip);
        Assert.Equal("VIP", estrategia.Nombre);
        Assert.Equal(6.00m, descuento.Amount);
    }

    [Fact]
    public void Carmen_MAYORISTA_P03x10_Descuento_15()
    {
        var (estrategia, descuento) = _calc.Calcular(new Money(100m), 10, CategoriaCliente.Normal);
        Assert.Equal("MAYORISTA", estrategia.Nombre);
        Assert.Equal(15.00m, descuento.Amount);
    }

    [Fact]
    public void Luis_NORMAL_Sin_Descuento()
    {
        var (estrategia, descuento) = _calc.Calcular(new Money(90m), 4, CategoriaCliente.Normal);
        Assert.Equal("NORMAL", estrategia.Nombre);
        Assert.Equal(0m, descuento.Amount);
    }

    [Fact]
    public void Mayorista_Prioridad_Sobre_VIP()
    {
        var (estrategia, _) = _calc.Calcular(new Money(200m), 12, CategoriaCliente.Vip);
        Assert.Equal("MAYORISTA", estrategia.Nombre);
    }
}