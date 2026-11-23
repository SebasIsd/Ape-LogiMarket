using LogiMarket.Domain.Entities;

namespace LogiMarket.Application.Ports.Persistence;

public interface IRepositorioCliente
{
    Task<Cliente?> ObtenerPorIdAsync(Guid id, CancellationToken ct = default);
    Task<Cliente?> ObtenerPorIdentificacionAsync(string identificacion, CancellationToken ct = default);
    Task<IReadOnlyList<Cliente>> ListarAsync(string? filtro = null, CancellationToken ct = default);
    Task AgregarAsync(Cliente cliente, CancellationToken ct = default);
    Task ActualizarAsync(Cliente cliente, CancellationToken ct = default);
}