using LogiMarket.Application.Ports.Persistence;
using LogiMarket.Domain.Entities.Catalog;
using LogiMarket.Domain.Exceptions;
using LogiMarket.Domain.ValueObjects;
using LogiMarket.Infrastructure.Persistence.Entities;
using Microsoft.EntityFrameworkCore;

namespace LogiMarket.Infrastructure.Persistence.Repositories;

public class RepositorioCatalogo : IRepositorioCatalogo
{
    private readonly AppDbContext _db;
    public RepositorioCatalogo(AppDbContext db) { _db = db; }

    public async Task<IElementoCatalogo?> ObtenerPorSkuAsync(string sku, CancellationToken ct = default)
    {
        var producto = await _db.Productos.AsNoTracking().FirstOrDefaultAsync(p => p.Sku == sku, ct);
        if (producto is not null) return producto;

        var kit = await _db.Kits.AsNoTracking().FirstOrDefaultAsync(k => k.Sku == sku, ct);
        if (kit is null) return null;

        await CargarComponentesAsync(kit, ct);
        return kit;
    }

    private async Task CargarComponentesAsync(KitCompuesto kit, CancellationToken ct)
    {
        // AsNoTracking evita que Include deje ProductoSimple trackeados en el DbContext del MainForm
        var componentes = await _db.KitComponentes
            .AsNoTracking()
            .Include(c => c.Producto)
            .Include(c => c.KitHijo)
            .Where(c => c.KitPadreId == kit.Id)
            .ToListAsync(ct);

        var campoComponentes = typeof(KitCompuesto)
            .GetField("_componentes", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)!;
        var lista = (System.Collections.IList)campoComponentes.GetValue(kit)!;
        lista.Clear();

        foreach (var ce in componentes)
        {
            var elemento = ce.ResolverElemento();
            if (elemento is KitCompuesto kitHijo)
                await CargarComponentesAsync(kitHijo, ct);
            lista.Add(new KitComponente(elemento, ce.Cantidad));
        }
    }

    public async Task<IReadOnlyList<IElementoCatalogo>> ListarTodoAsync(CancellationToken ct = default)
    {
        var productos = (IReadOnlyList<IElementoCatalogo>)await _db.Productos.AsNoTracking().ToListAsync(ct);
        var kits = await _db.Kits.AsNoTracking().ToListAsync(ct);
        foreach (var k in kits) await CargarComponentesAsync(k, ct);
        return productos.Concat<IElementoCatalogo>(kits).ToList().AsReadOnly();
    }

    public async Task<IReadOnlyList<ProductoSimple>> ListarProductosAsync(CancellationToken ct = default)
        => await _db.Productos.AsNoTracking().OrderBy(p => p.Sku).ToListAsync(ct);

    public async Task<IReadOnlyList<KitCompuesto>> ListarKitsAsync(CancellationToken ct = default)
    {
        var kits = await _db.Kits.AsNoTracking().ToListAsync(ct);
        foreach (var k in kits) await CargarComponentesAsync(k, ct);
        return kits;
    }

    public async Task AgregarProductoAsync(ProductoSimple producto, CancellationToken ct = default)
    {
        _db.Productos.Add(producto);
        await _db.SaveChangesAsync(ct);
    }

    public async Task AgregarKitAsync(KitCompuesto kit, CancellationToken ct = default)
    {
        _db.Kits.Add(kit);
        await PersistirKitAsync(kit, ct);
        await _db.SaveChangesAsync(ct);
    }

    public async Task ActualizarProductoAsync(ProductoSimple producto, CancellationToken ct = default)
    {
        foreach (var e in _db.ChangeTracker.Entries<ProductoSimple>()
                     .Where(e => e.Entity.Id == producto.Id)
                     .ToList())
        {
            e.State = EntityState.Detached;
        }

        var filas = await _db.Productos
            .Where(p => p.Id == producto.Id)
            .ExecuteUpdateAsync(
                s => s.SetProperty(p => p.Stock, producto.Stock),
                ct);

        if (filas == 0)
            throw new DomainException(
                $"No se pudo actualizar el stock del producto {producto.Sku} (Id={producto.Id}).");
    }

    private async Task PersistirKitAsync(KitCompuesto kit, CancellationToken ct)
    {
        foreach (var c in kit.Componentes)
        {
            var entity = new KitComponenteEntity
            {
                Id = Guid.NewGuid(),
                KitPadreId = kit.Id,
                Cantidad = c.Cantidad
            };
            if (c.Elemento is ProductoSimple p)
            {
                entity.TipoElemento = "Producto";
                entity.ProductoId = p.Id;
            }
            else if (c.Elemento is KitCompuesto kh)
            {
                entity.TipoElemento = "Kit";
                entity.KitHijoId = kh.Id;
                if (await _db.Kits.FindAsync(new object?[] { kh.Id }, ct) is null)
                {
                    _db.Kits.Add(kh);
                    await PersistirKitAsync(kh, ct);
                }
            }
            _db.KitComponentes.Add(entity);
        }
    }
}