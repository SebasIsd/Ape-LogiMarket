using LogiMarket.Application.Ports.Persistence;
using LogiMarket.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace LogiMarket.Infrastructure.Persistence.Repositories;

public class RepositorioCliente : IRepositorioCliente
{
    private readonly AppDbContext _db;
    public RepositorioCliente(AppDbContext db) { _db = db; }

    public async Task<Cliente?> ObtenerPorIdAsync(Guid id, CancellationToken ct = default)
        => await _db.Clientes.FirstOrDefaultAsync(c => c.Id == id, ct);

    public async Task<Cliente?> ObtenerPorIdentificacionAsync(string identificacion, CancellationToken ct = default)
        => await _db.Clientes.FirstOrDefaultAsync(c => c.Identificacion == identificacion, ct);

    public async Task<IReadOnlyList<Cliente>> ListarAsync(string? filtro = null, CancellationToken ct = default)
    {
        var q = _db.Clientes.AsQueryable();
        if (!string.IsNullOrWhiteSpace(filtro))
        {
            var f = filtro.Trim().ToLower();
            q = q.Where(c => c.Nombre.ToLower().Contains(f) || c.Identificacion.Contains(f));
        }
        return await q.OrderBy(c => c.Nombre).ToListAsync(ct);
    }

    public async Task AgregarAsync(Cliente cliente, CancellationToken ct = default)
    {
        _db.Clientes.Add(cliente);
        await _db.SaveChangesAsync(ct);
    }

    public async Task ActualizarAsync(Cliente cliente, CancellationToken ct = default)
    {
        _db.Clientes.Update(cliente);
        await _db.SaveChangesAsync(ct);
    }
}