using LogiMarket.Domain.Entities;

namespace LogiMarket.Application.Ports.Persistence;

public interface IRepositorioOutbox
{
    Task AgregarAsync(EventoOutbox evento, CancellationToken ct = default);
    Task<IReadOnlyList<EventoOutbox>> ObtenerPendientesAsync(int limite = 50, CancellationToken ct = default);
    Task MarcarProcesadoAsync(Guid id, CancellationToken ct = default);
    Task IncrementarReintentoAsync(Guid id, CancellationToken ct = default);
}