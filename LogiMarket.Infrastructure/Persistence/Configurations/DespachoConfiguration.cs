using LogiMarket.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace LogiMarket.Infrastructure.Persistence.Configurations;

public class DespachoConfiguration : IEntityTypeConfiguration<Despacho>
{
    public void Configure(EntityTypeBuilder<Despacho> b)
    {
        b.ToTable("Despachos");
        b.HasKey(x => x.Id);
        b.Property(x => x.Guia).HasMaxLength(60).IsRequired();
        b.HasIndex(x => x.Guia).IsUnique();
        b.Property(x => x.TipoEnvio).HasMaxLength(20);
        b.Property(x => x.PlazoDias);
        b.Property(x => x.FechaDespacho);
        b.Property(x => x.FechaEstimadaEntrega);
    }
}