using LogiMarket.Application.Validation;
using LogiMarket.Domain.Entities;
using LogiMarket.Domain.Entities.Catalog;
using LogiMarket.Domain.Enums;
using LogiMarket.Domain.ValueObjects;
using Xunit;

namespace LogiMarket.Tests.Application;

public class ChainTests
{
    [Fact]
    public async Task Cliente_Activo_Pasa_Validador()
    {
        // El dominio ya impide crear pedidos con clientes inactivos, 
        // así que probamos que un cliente activo pase el validador correctamente.
        var cliente = new Cliente("001", "Ana", "ana@test.com", CategoriaCliente.Vip);
        var p1 = new ProductoSimple("P01", "Mouse", new Money(20m), 8, 0.20m);
        var pedido = new Pedido(cliente.Id, cliente, TipoEnvio.Estandar);
        pedido.AgregarLinea(p1, 1);

        var validador = new ClienteActivoValidator();
        var resultado = await validador.ValidarAsync(pedido, CancellationToken.None);

        Assert.True(resultado.Ok);
    }

    [Fact]
    public async Task Lineas_Vacias_Detiene_Cadena()
    {
        var cliente = new Cliente("001", "Ana", "ana@test.com", CategoriaCliente.Vip);
        var pedido = new Pedido(cliente.Id, cliente, TipoEnvio.Estandar);
        // Sin líneas

        var validador = new LineasValidator();
        var resultado = await validador.ValidarAsync(pedido, CancellationToken.None);

        Assert.False(resultado.Ok);
        Assert.Contains("líneas", resultado.Error!.ToLower());
    }
}