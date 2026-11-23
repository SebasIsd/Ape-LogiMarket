using LogiMarket.Domain.Entities.Catalog;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace LogiMarket.Infrastructure.Persistence.Configurations;

public class ProductoSimpleConfiguration : IEntityTypeConfiguration<ProductoSimple>
{
    public void Configure(EntityTypeBuilder<ProductoSimple> b)
    {
        b.ToTable("Productos");
        b.HasKey(x => x.Id);
        b.HasIndex(x => x.Sku).IsUnique();
        b.Property(x => x.Sku).HasMaxLength(20).IsRequired();
        b.Property(x => x.Nombre).HasMaxLength(120).IsRequired();
        b.OwnsOne(x => x.Precio, p => p.Property(pp => pp.Amount).HasPrecision(18, 2).HasColumnName("Precio"));
        b.Property(x => x.Stock);
        b.Property(x => x.PesoKg).HasPrecision(10, 2);
        b.Ignore(x => x.EsKit);
    }
}