using LogiMarket.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace LogiMarket.Infrastructure.Persistence.Configurations;

public class PagoConfiguration : IEntityTypeConfiguration<Pago>
{
    public void Configure(EntityTypeBuilder<Pago> b)
    {
        b.ToTable("Pagos");
        b.HasKey(x => x.Id);
        b.HasIndex(x => x.ClaveIdempotencia).IsUnique();
        b.HasIndex(x => x.Referencia).IsUnique();
        b.Property(x => x.ClaveIdempotencia).HasMaxLength(80).IsRequired();
        b.Property(x => x.Referencia).HasMaxLength(80).IsRequired();
        b.Property(x => x.Proveedor).HasMaxLength(60);

        // CORREGIDO, para que EF Core pueda mapear correctamente el objeto de valor Money a un tipo decimal en la base de datos
        b.Property(x => x.Monto)
         .HasConversion(
             v => v.Amount,
             v => new Domain.ValueObjects.Money(v))
         .HasColumnType("DECIMAL(18,2)")
         .HasColumnName("Monto");

        b.Property(x => x.Fecha);
    }
}