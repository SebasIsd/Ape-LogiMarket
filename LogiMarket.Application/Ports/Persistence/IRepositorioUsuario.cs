using LogiMarket.Domain.Entities;
using LogiMarket.Domain.Enums;

namespace LogiMarket.Application.Ports.Persistence;

public interface IRepositorioUsuario
{
    Task<Usuario?> ObtenerPorUsernameAsync(string username, CancellationToken ct = default);
    Task<IReadOnlyList<Usuario>> ListarAsync(CancellationToken ct = default);
    Task AgregarAsync(Usuario usuario, CancellationToken ct = default);
    Task ActualizarAsync(Usuario usuario, CancellationToken ct = default);
}