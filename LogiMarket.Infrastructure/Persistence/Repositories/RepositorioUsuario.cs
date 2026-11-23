using LogiMarket.Application.Ports.Persistence;
using LogiMarket.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace LogiMarket.Infrastructure.Persistence.Repositories;

public class RepositorioUsuario : IRepositorioUsuario
{
    private readonly AppDbContext _db;
    public RepositorioUsuario(AppDbContext db) { _db = db; }

    public async Task<Usuario?> ObtenerPorUsernameAsync(string username, CancellationToken ct = default)
        => await _db.Usuarios.FirstOrDefaultAsync(u => u.Username == username, ct);

    public async Task<IReadOnlyList<Usuario>> ListarAsync(CancellationToken ct = default)
        => await _db.Usuarios.OrderBy(u => u.Username).ToListAsync(ct);

    public async Task AgregarAsync(Usuario usuario, CancellationToken ct = default)
    {
        _db.Usuarios.Add(usuario);
        await _db.SaveChangesAsync(ct);
    }

    public async Task ActualizarAsync(Usuario usuario, CancellationToken ct = default)
    {
        _db.Usuarios.Update(usuario);
        await _db.SaveChangesAsync(ct);
    }
}