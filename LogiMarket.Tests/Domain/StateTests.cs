using LogiMarket.Domain.Entities;
using LogiMarket.Domain.Entities.Catalog;
using LogiMarket.Domain.Enums;
using LogiMarket.Domain.Exceptions;
using LogiMarket.Domain.ValueObjects;
using Xunit;

namespace LogiMarket.Tests.Domain;

public class StateTests
{
    private Pedido CrearPedidoValido()
    {
        var cliente = new Cliente("001", "Ana", "ana@test.com", CategoriaCliente.Vip);
        var p1 = new ProductoSimple("P01", "Mouse", new Money(20m), 8, 0.20m);
        var pedido = new Pedido(cliente.Id, cliente, TipoEnvio.Estandar);
        pedido.AgregarLinea(p1, 1);
        return pedido;
    }

    [Fact]
    public void Ciclo_Valido_Borrador_a_Entregado()
    {
        var p = CrearPedidoValido();
        Assert.Equal("BORRADOR", p.EstadoActual);

        p.Validar();
        Assert.Equal("VALIDADO", p.EstadoActual);

        p.Pagar(new Pago(p.Id, p.Total, "REF-1", "k1", "Test"));
        Assert.Equal("PAGADO", p.EstadoActual);

        p.Despachar(new Despacho(p.Id, "GUIA-1", "ESTÁNDAR", 3));
        Assert.Equal("DESPACHADO", p.EstadoActual);

        p.Entregar();
        Assert.Equal("ENTREGADO", p.EstadoActual);
    }

    [Fact]
    public void Pagado_a_Cancelado_Lanza_Excepcion()
    {
        var p = CrearPedidoValido();
        p.Validar();
        p.Pagar(new Pago(p.Id, p.Total, "REF-1", "k1", "Test"));

        Assert.Throws<OperacionNoPermitidaException>(() => p.Cancelar());
    }

    [Fact]
    public void Solo_Borrador_Es_Editable()
    {
        var p = CrearPedidoValido();
        var producto = new ProductoSimple("P02", "Teclado", new Money(30m), 5, 0.70m);

        p.AgregarLinea(producto, 1); // OK en BORRADOR
        p.Validar();

        Assert.Throws<OperacionNoPermitidaException>(() => p.AgregarLinea(producto, 1));
    }
}