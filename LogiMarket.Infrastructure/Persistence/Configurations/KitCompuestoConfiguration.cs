using LogiMarket.Domain.Entities.Catalog;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace LogiMarket.Infrastructure.Persistence.Configurations;

public class KitCompuestoConfiguration : IEntityTypeConfiguration<KitCompuesto>
{
    public void Configure(EntityTypeBuilder<KitCompuesto> b)
    {
        b.ToTable("Kits");
        b.HasKey(x => x.Id);
        b.HasIndex(x => x.Sku).IsUnique();
        b.Property(x => x.Sku).HasMaxLength(30).IsRequired();
        b.Property(x => x.Nombre).HasMaxLength(120).IsRequired();
        b.Ignore(x => x.EsKit);
        b.Ignore(x => x.Componentes); // mapeamos vía KitComponenteEntity
    }
}