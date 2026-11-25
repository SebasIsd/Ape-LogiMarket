using LogiMarket.Domain.Builders;
using LogiMarket.Domain.Entities;
using LogiMarket.Domain.Entities.Catalog;
using LogiMarket.Domain.Enums;
using LogiMarket.Domain.Exceptions;
using LogiMarket.Domain.ValueObjects;
using Xunit;

namespace LogiMarket.Tests.Application;

public class BuilderTests
{
    [Fact]
    public void Builder_Sin_Cliente_Lanza_Excepcion()
    {
        var builder = new PedidoBuilder();
        Assert.Throws<DomainException>(() => builder.Construir());
    }

    [Fact]
    public void Builder_Sin_Lineas_Lanza_Excepcion()
    {
        var cliente = new Cliente("001", "Ana", "ana@test.com", CategoriaCliente.Vip);
        var builder = new PedidoBuilder().ConCliente(cliente);
        Assert.Throws<DomainException>(() => builder.Construir());
    }

    [Fact]
    public void Builder_Completo_Crea_Pedido_Valido()
    {
        var cliente = new Cliente("001", "Ana", "ana@test.com", CategoriaCliente.Vip);
        var p1 = new ProductoSimple("P01", "Mouse", new Money(20m), 8, 0.20m);
        var builder = new PedidoBuilder()
            .ConCliente(cliente)
            .ConEnvio(TipoEnvio.Estandar)
            .AgregarLinea(p1, 2) // ← Agrega 1 línea con cantidad 2
            .ConDescuento(new Money(4m), "VIP")
            .ConTarifa(new Money(5m));

        var pedido = builder.Construir();

        Assert.Equal("BORRADOR", pedido.EstadoActual);
        Assert.Equal(1, pedido.Detalles.Count); // ← Corregido: 1 línea
        Assert.Equal(2, pedido.Detalles.First().Cantidad); // ← Corregido: cantidad 2
        Assert.Equal(41.00m, pedido.Total.Amount); // 40 - 4 + 5
    }
}