using LogiMarket.Application.Ports.Persistence;
using LogiMarket.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace LogiMarket.Infrastructure.Persistence.Repositories;

public class RepositorioAuditoria : IRepositorioAuditoria
{
    private readonly AppDbContext _db;
    public RepositorioAuditoria(AppDbContext db) { _db = db; }

    public async Task AgregarAsync(Auditoria registro, CancellationToken ct = default)
    {
        _db.Auditorias.Add(registro);
        await _db.SaveChangesAsync(ct);
    }

    public async Task<IReadOnlyList<Auditoria>> ListarUltimosAsync(int limite = 100, CancellationToken ct = default)
        => await _db.Auditorias.OrderByDescending(a => a.Fecha).Take(limite).ToListAsync(ct);
}