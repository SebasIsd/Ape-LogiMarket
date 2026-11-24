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
        }

        await _db.SaveChangesAsync(ct);
    }

    public async Task ActualizarAsync(Pedido pedido, CancellationToken ct = default)
    {
        _db.Pedidos.Update(pedido);
        // Sincronizar detalles
        var existentes = await _db.PedidoDetalles.Where(d => d.PedidoId == pedido.Id).ToListAsync(ct);
        _db.PedidoDetalles.RemoveRange(existentes);
        foreach (var d in pedido.Detalles) _db.PedidoDetalles.Add(d);
        await _db.SaveChangesAsync(ct);
    }

    public async Task<bool> ExistePagoPorClaveAsync(string clave, CancellationToken ct = default)
        => await _db.Pagos.AnyAsync(p => p.ClaveIdempotencia == clave, ct);

    public async Task<bool> ExisteDevolucionParaPedidoAsync(Guid pedidoId, CancellationToken ct = default)
        => await _db.Devoluciones.AnyAsync(d => d.PedidoId == pedidoId, ct);
}