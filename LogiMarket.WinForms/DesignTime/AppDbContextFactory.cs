using LogiMarket.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace LogiMarket.WinForms.DesignTime;

/// <summary>
/// Esta fábrica es usada ÚNICAMENTE por las herramientas de EF Core en tiempo de diseño.
/// Al estar en el proyecto de inicio (WinForms), EF Core la encontrará y NO ejecutará Program.cs.
/// </summary>
public class AppDbContextFactory : IDesignTimeDbContextFactory<AppDbContext>
{
    public AppDbContext CreateDbContext(string[] args)
    {
        var optionsBuilder = new DbContextOptionsBuilder<AppDbContext>();

        // Usamos la misma cadena de conexión. No se ejecutará el Seed aquí.
        optionsBuilder.UseSqlite("Data Source=logimarket.db");

        return new AppDbContext(optionsBuilder.Options);
    }
}