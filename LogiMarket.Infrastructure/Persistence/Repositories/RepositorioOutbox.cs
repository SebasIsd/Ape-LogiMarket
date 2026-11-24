using LogiMarket.Application.Ports.Persistence;
using LogiMarket.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace LogiMarket.Infrastructure.Persistence.Repositories;

public class RepositorioOutbox : IRepositorioOutbox
{
    private readonly AppDbContext _db;
    public RepositorioOutbox(AppDbContext db) { _db = db; }

    public async Task AgregarAsync(EventoOutbox evento, CancellationToken ct = default)
    {
        _db.Outbox.Add(evento);
        await _db.SaveChangesAsync(ct);
    }

    public async Task<IReadOnlyList<EventoOutbox>> ObtenerPendientesAsync(int limite = 50, CancellationToken ct = default)
        => await _db.Outbox
            .Where(e => !e.Procesado)
            .OrderBy(e => e.CreadoEn)
            .Take(limite)
            .ToListAsync(ct);

    public async Task<IReadOnlyList<EventoOutbox>> ListarRecientesAsync(int limite = 100, CancellationToken ct = default)
        => await _db.Outbox
            .AsNoTracking()
            .OrderByDescending(e => e.CreadoEn)
            .Take(limite)
            .ToListAsync(ct);

    public async Task MarcarProcesadoAsync(Guid id, CancellationToken ct = default)
    {
        var e = await _db.Outbox.FindAsync(new object?[] { id }, ct)
            ?? throw new InvalidOperationException("Evento no encontrado");
        e.MarcarProcesado();
        await _db.SaveChangesAsync(ct);
    }

    public async Task IncrementarReintentoAsync(Guid id, CancellationToken ct = default)
    {
        var e = await _db.Outbox.FindAsync(new object?[] { id }, ct)
            ?? throw new InvalidOperationException("Evento no encontrado");
        e.IncrementarReintento();
        await _db.SaveChangesAsync(ct);
    }
}