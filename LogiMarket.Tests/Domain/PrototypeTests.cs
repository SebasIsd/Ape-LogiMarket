using LogiMarket.Domain.Entities;
using LogiMarket.Domain.Entities.Catalog;
using LogiMarket.Domain.Enums;
using LogiMarket.Domain.Exceptions;
using LogiMarket.Domain.ValueObjects;
using Xunit;

namespace LogiMarket.Tests.Domain;

public class PrototypeTests
{
    [Fact]
    public void Clone_Copia_Profunda_De_Detalles()
    {
        var cliente = new Cliente("001", "Ana", "ana@test.com", CategoriaCliente.Vip);
        var p1 = new ProductoSimple("P01", "Mouse", new Money(20m), 8, 0.20m);
        var pedido = new Pedido(cliente.Id, cliente, TipoEnvio.Estandar);
        pedido.AgregarLinea(p1, 2); // ← 1 línea, cantidad 2

        var clon = (Pedido)pedido.Clone();

        Assert.NotEqual(pedido.Id, clon.Id);
        Assert.Equal(1, clon.Detalles.Count); // ← Corregido: 1 línea
        Assert.Equal(2, clon.Detalles.First().Cantidad); // ← Corregido: cantidad 2
        Assert.Equal(pedido.Detalles.First().Cantidad, clon.Detalles.First().Cantidad);
    }

    [Fact]
    public void Clone_Solo_Permitido_En_Borrador()
    {
        var cliente = new Cliente("001", "Ana", "ana@test.com", CategoriaCliente.Vip);
        var p1 = new ProductoSimple("P01", "Mouse", new Money(20m), 8, 0.20m);
        var pedido = new Pedido(cliente.Id, cliente, TipoEnvio.Estandar);
        pedido.AgregarLinea(p1, 1);
        pedido.Validar();

        Assert.Throws<OperacionNoPermitidaException>(() => pedido.Clone());
    }
}