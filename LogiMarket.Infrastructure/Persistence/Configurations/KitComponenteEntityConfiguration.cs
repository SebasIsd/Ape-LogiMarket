using LogiMarket.Infrastructure.Persistence.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace LogiMarket.Infrastructure.Persistence.Configurations;

public class KitComponenteEntityConfiguration : IEntityTypeConfiguration<KitComponenteEntity>
{
    public void Configure(EntityTypeBuilder<KitComponenteEntity> b)
    {
        b.ToTable("KitComponentes");
        b.HasKey(x => x.Id);
        b.Property(x => x.TipoElemento).HasMaxLength(10).IsRequired();
        b.Property(x => x.Cantidad).IsRequired();

        b.HasOne(x => x.KitPadre)
           .WithMany()
           .HasForeignKey(x => x.KitPadreId)
           .OnDelete(DeleteBehavior.Cascade);

        b.HasOne(x => x.Producto)
           .WithMany()
           .HasForeignKey(x => x.ProductoId)
           .OnDelete(DeleteBehavior.Restrict)
           .IsRequired(false);

        b.HasOne(x => x.KitHijo)
           .WithMany()
           .HasForeignKey(x => x.KitHijoId)
           .OnDelete(DeleteBehavior.Restrict)
           .IsRequired(false);

        //reemplazado para que solo se pueda insertar Producto o Kit, no ambos
        b.ToTable("KitComponentes", t => t.HasCheckConstraint("CK_TipoElemento", "TipoElemento IN ('Producto','Kit')"));
    }
}