using LogiMarket.Application.Ports.Persistence;
using LogiMarket.Domain.Entities;
using LogiMarket.Domain.Entities.Catalog;
using LogiMarket.Infrastructure.Persistence.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;

namespace LogiMarket.Infrastructure.Persistence;

public class AppDbContext : DbContext, IUnitOfWork
{
    public DbSet<ProductoSimple> Productos => Set<ProductoSimple>();
    public DbSet<KitCompuesto> Kits => Set<KitCompuesto>();
    public DbSet<KitComponenteEntity> KitComponentes => Set<KitComponenteEntity>();
    public DbSet<Cliente> Clientes => Set<Cliente>();
    public DbSet<Usuario> Usuarios => Set<Usuario>();
    public DbSet<Pedido> Pedidos => Set<Pedido>();
    public DbSet<PedidoDetalle> PedidoDetalles => Set<PedidoDetalle>();
    public DbSet<Pago> Pagos => Set<Pago>();
    public DbSet<MovimientoInventario> Movimientos => Set<MovimientoInventario>();
    public DbSet<Despacho> Despachos => Set<Despacho>();
    public DbSet<Devolucion> Devoluciones => Set<Devolucion>();
    public DbSet<EventoOutbox> Outbox => Set<EventoOutbox>();
    public DbSet<Auditoria> Auditorias => Set<Auditoria>();

    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(AppDbContext).Assembly);
        base.OnModelCreating(modelBuilder);
    }

    // IUnitOfWork
    public async Task<ITransaccion> BeginTransactionAsync(CancellationToken ct = default)
    {
        var tx = await Database.BeginTransactionAsync(ct);
        return new EfTransaction(tx);
    }

    async Task<int> IUnitOfWork.SaveChangesAsync(CancellationToken ct)
        => await SaveChangesAsync(ct);

    private sealed class EfTransaction : ITransaccion
    {
        private readonly IDbContextTransaction _tx;
        public EfTransaction(IDbContextTransaction tx) { _tx = tx; }
        public Task CommitAsync(CancellationToken ct = default) => _tx.CommitAsync(ct);
        public Task RollbackAsync(CancellationToken ct = default) => _tx.RollbackAsync(ct);
        public ValueTask DisposeAsync() => _tx.DisposeAsync();
    }
}