using LogiMarket.Application.Ports.Persistence;
using LogiMarket.Domain.Entities;
using LogiMarket.Domain.States;
using Microsoft.EntityFrameworkCore;

namespace LogiMarket.Infrastructure.Persistence.Repositories;

public class RepositorioPedido : IRepositorioPedido
{
    private readonly AppDbContext _db;
    public RepositorioPedido(AppDbContext db) { _db = db; }

    public async Task<Pedido?> ObtenerPorIdAsync(Guid id, CancellationToken ct = default)
    {
        var pedido = await _db.Pedidos
            .Include(p => p.Cliente)
            .Include(p => p.Pago)
            .Include(p => p.Despacho)
            .Include(p => p.Devolucion)
            .FirstOrDefaultAsync(p => p.Id == id, ct);

        if (pedido is null) return null;

        // Cargar detalles
        var detalles = await _db.PedidoDetalles
            .Where(d => d.PedidoId == id)
            .ToListAsync(ct);

        var campoDetalles = typeof(Pedido)
            .GetField("_detalles", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)!;
        var lista = (System.Collections.IList)campoDetalles.GetValue(pedido)!;
        lista.Clear();
        foreach (var d in detalles) lista.Add(d);

        // Restaurar estado
        IEstadoPedido estado = pedido.EstadoActual switch
        {
            "BORRADOR" => new Domain.States.BorradorState(),
            "VALIDADO" => new Domain.States.ValidadoState(),
            "PAGADO" => new Domain.States.PagadoState(),
            "DESPACHADO" => new Domain.States.DespachadoState(),
            "ENTREGADO" => new Domain.States.EntregadoState(),
            "CANCELADO" => new Domain.States.CanceladoState(),
            _ => new Domain.States.BorradorState()
        };
        var campoEstado = typeof(Pedido)
            .GetField("_estado", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)!;
        campoEstado.SetValue(pedido, estado);

        return pedido;
    }

    public async Task<IReadOnlyList<Pedido>> ListarAsync(CancellationToken ct = default)
    {
        var pedidos = await _db.Pedidos
            .Include(p => p.Cliente)
            .Include(p => p.Pago)
            .Include(p => p.Despacho)
            .Include(p => p.Devolucion)
            .OrderByDescending(p => p.FechaCreacion)
            .ToListAsync(ct);

        foreach (var p in pedidos)
        {
            var detalles = await _db.PedidoDetalles.Where(d => d.PedidoId == p.Id).ToListAsync(ct);
            var campoDetalles = typeof(Pedido)
                .GetField("_detalles", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)!;
            var lista = (System.Collections.IList)campoDetalles.GetValue(p)!;
            lista.Clear();
            foreach (var d in detalles) lista.Add(d);

            IEstadoPedido estado = p.EstadoActual switch
            {
                "BORRADOR" => new Domain.States.BorradorState(),
                "VALIDADO" => new Domain.States.ValidadoState(),
                "PAGADO" => new Domain.States.PagadoState(),
                "DESPACHADO" => new Domain.States.DespachadoState(),
                "ENTREGADO" => new Domain.States.EntregadoState(),
                "CANCELADO" => new Domain.States.CanceladoState(),
                _ => new Domain.States.BorradorState()
            };
            var campoEstado = typeof(Pedido)
                .GetField("_estado", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)!;
            campoEstado.SetValue(p, estado);
        }

        return pedidos.AsReadOnly();
    }

    public async Task AgregarAsync(Pedido pedido, CancellationToken ct = default)
    {
        // Adjuntar el cliente existente (ya está en BD) para que EF no lo duplique
        if (pedido.Cliente is not null)
        {
            _db.Clientes.Attach(pedido.Cliente);
            _db.Entry(pedido.Cliente).State = EntityState.Unchanged;
        }

        _db.Pedidos.Add(pedido);

        foreach (var d in pedido.Detalles)
        {
            _db.PedidoDetalles.Add(d);
            // PedidoDetalle no tiene navegación hacia Pedido: la FK se asigna explícitamente
            _db.Entry(d).Property(x => x.PedidoId).CurrentValue = pedido.Id;
        }

        await _db.SaveChangesAsync(ct);
    }

    public async Task ActualizarAsync(Pedido pedido, CancellationToken ct = default)
    {
        var entry = _db.Entry(pedido);
        if (entry.State == EntityState.Detached)
            _db.Pedidos.Update(pedido);

        // EF marca como "Modified" toda entidad nueva cuyo Guid ya viene asignado desde el constructor
        // (Pago, Despacho, Devolución, líneas), lo que produce un UPDATE de 0 filas y la
        // DbUpdateConcurrencyException. Se consulta la BD y se fija Added o Modified según exista la fila.
        async Task CorregirEstadoAsync(object? entidad)
        {
            if (entidad is null) return;
            var e = _db.Entry(entidad);
            if (e.State == EntityState.Detached) e.State = EntityState.Unchanged;
            var enBd = await e.GetDatabaseValuesAsync(ct);   // null => la fila no existe
            e.State = enBd is null ? EntityState.Added : EntityState.Modified;
        }

        await CorregirEstadoAsync(pedido.Pago);
        await CorregirEstadoAsync(pedido.Despacho);
        await CorregirEstadoAsync(pedido.Devolucion);

        foreach (var d in pedido.Detalles)
        {
            await CorregirEstadoAsync(d);
            _db.Entry(d).Property(x => x.PedidoId).CurrentValue = pedido.Id;
        }

        try
        {
            await _db.SaveChangesAsync(ct);
        }
        catch (DbUpdateConcurrencyException ex)
        {
            var entidades = string.Join(", ", ex.Entries.Select(x => $"{x.Entity.GetType().Name}={x.State}"));
            throw new InvalidOperationException($"Conflicto al guardar el pedido. Entidades: {entidades}", ex);
        }
    }

    public async Task<bool> ExistePagoPorClaveAsync(string clave, CancellationToken ct = default)
        => await _db.Pagos.AnyAsync(p => p.ClaveIdempotencia == clave, ct);

    public async Task<bool> ExisteDevolucionParaPedidoAsync(Guid pedidoId, CancellationToken ct = default)
        => await _db.Devoluciones.AnyAsync(d => d.PedidoId == pedidoId, ct);
}