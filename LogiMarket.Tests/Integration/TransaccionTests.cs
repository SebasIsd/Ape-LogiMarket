using LogiMarket.Domain.Entities;
using LogiMarket.Domain.Enums;
using LogiMarket.Infrastructure.Persistence.Repositories;
using Xunit;

namespace LogiMarket.Tests.Integration;

public class TransaccionTests
{
    [Fact]
    public async Task Agregar_Y_Obtener_Cliente()
    {
        using var db = await TestDbFactory.CrearAsync();
        var repo = new RepositorioCliente(db);

        var cliente = new Cliente("002", "Luis", "luis@test.com", CategoriaCliente.Normal);
        await repo.AgregarAsync(cliente);

        var obtenido = await repo.ObtenerPorIdentificacionAsync("002");

        Assert.NotNull(obtenido);
        Assert.Equal("Luis", obtenido!.Nombre);
        Assert.Equal(CategoriaCliente.Normal, obtenido.Categoria);
    }
}