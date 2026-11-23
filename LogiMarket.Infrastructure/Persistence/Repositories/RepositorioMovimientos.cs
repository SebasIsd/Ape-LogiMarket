using LogiMarket.Application.Ports.Persistence;
using LogiMarket.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace LogiMarket.Infrastructure.Persistence.Repositories;

public class RepositorioMovimientos : IRepositorioMovimientos
{
    private readonly AppDbContext _db;
    public RepositorioMovimientos(AppDbContext db) { _db = db; }

    public async Task AgregarAsync(MovimientoInventario movimiento, CancellationToken ct = default)
    {
        _db.Movimientos.Add(movimiento);
        await _db.SaveChangesAsync(ct);
    }

    public async Task<IReadOnlyList<MovimientoInventario>> ListarPorSkuAsync(string sku, CancellationToken ct = default)
        => await _db.Movimientos.Where(m => m.Sku == sku).OrderByDescending(m => m.Fecha).ToListAsync(ct);

    public async Task<IReadOnlyList<MovimientoInventario>> ListarTodosAsync(CancellationToken ct = default)
        => await _db.Movimientos.OrderByDescending(m => m.Fecha).ToListAsync(ct);
}