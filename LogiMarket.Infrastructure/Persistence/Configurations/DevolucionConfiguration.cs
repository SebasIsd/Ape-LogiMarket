using LogiMarket.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace LogiMarket.Infrastructure.Persistence.Configurations;

public class DevolucionConfiguration : IEntityTypeConfiguration<Devolucion>
{
    public void Configure(EntityTypeBuilder<Devolucion> b)
    {
        b.ToTable("Devoluciones");
        b.HasKey(x => x.Id);
        b.HasIndex(x => x.PedidoId).IsUnique();
        b.Property(x => x.Motivo).HasConversion<string>().HasMaxLength(20);

        // CORREGIDO: usar HasConversion en lugar de OwnsOne
        b.Property(x => x.Reembolso)
         .HasConversion(
             v => v.Amount,
             v => new Domain.ValueObjects.Money(v))
         .HasColumnType("DECIMAL(18,2)")
         .HasColumnName("Reembolso");

        b.Property(x => x.FechaSolicitud);
        b.Property(x => x.Aceptada);
    }
}