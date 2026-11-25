using LogiMarket.Domain.Entities;
using LogiMarket.Domain.Entities.Catalog;
using LogiMarket.Domain.Enums;
using LogiMarket.Domain.ValueObjects;
using LogiMarket.Infrastructure.Persistence.Repositories;
using Xunit;

namespace LogiMarket.Tests.Integration;

public class RepositorioPedidoTests
{
    [Fact]
    public async Task Agregar_Y_Obtener_Pedido()
    {
        using var db = await TestDbFactory.CrearAsync();
        var repo = new RepositorioPedido(db);

        var cliente = new Cliente("001", "Ana", "ana@test.com", CategoriaCliente.Vip);
        db.Clientes.Add(cliente);
        await db.SaveChangesAsync();

        var p1 = new ProductoSimple("P01", "Mouse", new Money(20m), 8, 0.20m);
        var pedido = new Pedido(cliente.Id, cliente, TipoEnvio.Estandar);
        pedido.AgregarLinea(p1, 2); // ← 1 línea, cantidad 2
        pedido.Validar();

        await repo.AgregarAsync(pedido);

        var obtenido = await repo.ObtenerPorIdAsync(pedido.Id);
        Assert.NotNull(obtenido);
        Assert.Equal("VALIDADO", obtenido!.EstadoActual);
        Assert.Equal(1, obtenido.Detalles.Count); // ← Corregido: 1 línea
        Assert.Equal(2, obtenido.Detalles.First().Cantidad); // ← Corregido: cantidad 2
    }

    [Fact]
    public async Task Idempotencia_Bloquea_Doble_Pago()
    {
        using var db = await TestDbFactory.CrearAsync();
        var repo = new RepositorioPedido(db);

        db.Pagos.Add(new Pago(Guid.NewGuid(), new Money(59m), "REF-1", "k-123", "Test"));
        await db.SaveChangesAsync();

        Assert.True(await repo.ExistePagoPorClaveAsync("k-123"));
        Assert.False(await repo.ExistePagoPorClaveAsync("k-456"));
    }
}