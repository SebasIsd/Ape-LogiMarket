using LogiMarket.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace LogiMarket.Infrastructure.Persistence.Configurations;

public class MovimientoConfiguration : IEntityTypeConfiguration<MovimientoInventario>
{
    public void Configure(EntityTypeBuilder<MovimientoInventario> b)
    {
        b.ToTable("MovimientosInventario");
        b.HasKey(x => x.Id);
        b.Property(x => x.Sku).HasMaxLength(30).IsRequired();
        b.HasIndex(x => x.Sku);
        b.Property(x => x.Cantidad);
        b.Property(x => x.Tipo).HasConversion<string>().HasMaxLength(10);
        b.Property(x => x.Motivo).HasMaxLength(120);
        b.Property(x => x.Referencia).HasMaxLength(80);
        b.Property(x => x.Fecha);
    }
}