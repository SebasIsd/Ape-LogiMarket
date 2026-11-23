using LogiMarket.Domain.Entities;
using LogiMarket.Domain.Entities.Catalog;
using LogiMarket.Domain.Enums;
using LogiMarket.Domain.ValueObjects;
using LogiMarket.Infrastructure.Persistence.Entities;
using LogiMarket.Infrastructure.Security;
using Microsoft.EntityFrameworkCore;

namespace LogiMarket.Infrastructure.Persistence.Seed;

public static class AppSeeder
{
    public static async Task SeedAsync(AppDbContext db)
    {
        // Productos
        if (!await db.Productos.AnyAsync())
        {
            db.Productos.AddRange(
                new ProductoSimple("P01", "Mouse USB",    new Money(20m), 8,  0.20m),
                new ProductoSimple("P02", "Teclado",      new Money(30m), 5,  0.70m),
                new ProductoSimple("P03", "Memoria USB",  new Money(10m), 20, 0.10m),
                new ProductoSimple("P04", "Webcam",       new Money(40m), 4,  0.35m),
                new ProductoSimple("P05", "Alfombrilla",  new Money(5m),  30, 0.10m)
            );
            await db.SaveChangesAsync();
        }

        // Kits
        if (!await db.Kits.AnyAsync())
        {
            var p1 = await db.Productos.FirstAsync(p => p.Sku == "P01");
            var p2 = await db.Productos.FirstAsync(p => p.Sku == "P02");
            var p3 = await db.Productos.FirstAsync(p => p.Sku == "P03");

            var kitOficina = new KitCompuesto("KIT_OFICINA", "Kit Oficina");
            kitOficina.AgregarComponente(p1, 1);
            kitOficina.AgregarComponente(p2, 1);

            var kitTeletrabajo = new KitCompuesto("KIT_TELETRABAJO", "Kit Teletrabajo");
            kitTeletrabajo.AgregarComponente(kitOficina, 1);
            kitTeletrabajo.AgregarComponente(p3, 1);

            db.Kits.AddRange(kitOficina, kitTeletrabajo);
            await db.SaveChangesAsync();

            // Persistir componentes
            foreach (var c in kitOficina.Componentes)
            {
                var prod = c.Elemento as ProductoSimple;
                db.KitComponentes.Add(new KitComponenteEntity
                {
                    Id = Guid.NewGuid(),
                    KitPadreId = kitOficina.Id,
                    TipoElemento = "Producto",
                    ProductoId = prod!.Id,
                    Cantidad = c.Cantidad
                });
            }
            foreach (var c in kitTeletrabajo.Componentes)
            {
                var entity = new KitComponenteEntity
                {
                    Id = Guid.NewGuid(),
                    KitPadreId = kitTeletrabajo.Id,
                    Cantidad = c.Cantidad
                };
                if (c.Elemento is KitCompuesto k)
                {
                    entity.TipoElemento = "Kit";
                    entity.KitHijoId = k.Id;
                }
                else if (c.Elemento is ProductoSimple p)
                {
                    entity.TipoElemento = "Producto";
                    entity.ProductoId = p.Id;
                }
                db.KitComponentes.Add(entity);
            }
            await db.SaveChangesAsync();
        }

        // Usuarios
        if (!await db.Usuarios.AnyAsync())
        {
            db.Usuarios.AddRange(
                new Usuario("admin",    PasswordHasher.Hash("admin123"),   RolUsuario.Admin),
                new Usuario("vendedor", PasswordHasher.Hash("vendedor123"), RolUsuario.Ventas),
                new Usuario("bodega",   PasswordHasher.Hash("bodega123"),  RolUsuario.Bodega),
                new Usuario("consulta", PasswordHasher.Hash("consulta123"), RolUsuario.Consulta)
            );
            await db.SaveChangesAsync();
        }
    }
}