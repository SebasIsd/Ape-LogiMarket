using LogiMarket.Domain.Entities;

namespace LogiMarket.Application.Ports.Persistence;

public interface IRepositorioAuditoria
{
    Task AgregarAsync(Auditoria registro, CancellationToken ct = default);
    Task<IReadOnlyList<Auditoria>> ListarUltimosAsync(int limite = 100, CancellationToken ct = default);
}