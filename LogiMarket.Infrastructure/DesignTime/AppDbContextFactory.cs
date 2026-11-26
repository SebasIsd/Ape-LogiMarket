using LogiMarket.Application.Ports.Persistence;
using LogiMarket.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace LogiMarket.Infrastructure.DesignTime;

/// <summary>
/// Fábrica de DbContext usada ÚNICAMENTE por las herramientas de EF Core en tiempo de diseño
/// (dotnet ef migrations, dotnet ef database update).
/// No se usa en tiempo de ejecución; ahí el contexto proviene del contenedor DI de Program.cs.
/// </summary>
public class AppDbContextFactory : IDesignTimeDbContextFactory<AppDbContext>
{
    public AppDbContext CreateDbContext(string[] args)
    {
        var optionsBuilder = new DbContextOptionsBuilder<AppDbContext>();
        optionsBuilder.UseSqlite(DatabaseConfig.ConnectionString);
        return new AppDbContext(optionsBuilder.Options);
    }
}