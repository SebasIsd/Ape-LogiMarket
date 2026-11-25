using LogiMarket.Domain.Entities;
using LogiMarket.Infrastructure.Persistence.Repositories;
using Xunit;

namespace LogiMarket.Tests.Integration;

public class OutboxTests
{
    [Fact]
    public async Task Outbox_Registra_Y_Marca_Procesado()
    {
        using var db = await TestDbFactory.CrearAsync();
        var repo = new RepositorioOutbox(db);

        var evento = new EventoOutbox("PedidoPagado", "{\"Id\":\"test\"}");
        await repo.AgregarAsync(evento);

        var pendientes = await repo.ObtenerPendientesAsync();
        Assert.Single(pendientes);

        await repo.MarcarProcesadoAsync(evento.Id);

        pendientes = await repo.ObtenerPendientesAsync();
        Assert.Empty(pendientes);
    }
}