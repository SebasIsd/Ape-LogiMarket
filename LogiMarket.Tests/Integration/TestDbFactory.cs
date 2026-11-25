using LogiMarket.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace LogiMarket.Tests.Integration;

public static class TestDbFactory
{
    public static async Task<AppDbContext> CrearAsync()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        var db = new AppDbContext(options);
        await db.Database.EnsureCreatedAsync();
        return db;
    }
}