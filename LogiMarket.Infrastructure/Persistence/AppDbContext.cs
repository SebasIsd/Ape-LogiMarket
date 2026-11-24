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

        // Todas las claves Guid se asignan en el constructor del dominio (Guid.NewGuid()).
        // Con ValueGeneratedOnAdd (convención) EF trata una entidad nueva con clave ya asignada
        // como "Modified" al descubrirla por navegación (Pedido.Pago, etc.) y emite un UPDATE de 0 filas
        // => DbUpdateConcurrencyException. Con ValueGeneratedNever pasa a "Added" y hace INSERT.
        foreach (var entidad in modelBuilder.Model.GetEntityTypes())
        {
            var clave = entidad.FindPrimaryKey();
            if (clave is { Properties.Count: 1 } && clave.Properties[0].ClrType == typeof(Guid))
                clave.Properties[0].ValueGenerated = Microsoft.EntityFrameworkCore.Metadata.ValueGenerated.Never;
        }
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