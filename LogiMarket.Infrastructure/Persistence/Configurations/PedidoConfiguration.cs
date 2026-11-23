using LogiMarket.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace LogiMarket.Infrastructure.Persistence.Configurations;

public class PedidoConfiguration : IEntityTypeConfiguration<Pedido>
{
    public void Configure(EntityTypeBuilder<Pedido> b)
    {
        b.ToTable("Pedidos");
        b.HasKey(x => x.Id);
        b.Property(x => x.EstadoActual).HasMaxLength(20).IsRequired();
        b.Property(x => x.TipoEnvio).HasMaxLength(20).IsRequired();
        b.Property(x => x.EstrategiaDescuentoAplicada).HasMaxLength(20);

        // CORREGIDO: todos los Money con HasConversion
        b.Property(x => x.Subtotal)
         .HasConversion(v => v.Amount, v => new Domain.ValueObjects.Money(v))
         .HasColumnType("DECIMAL(18,2)")
         .HasColumnName("Subtotal");

        b.Property(x => x.Descuento)
         .HasConversion(v => v.Amount, v => new Domain.ValueObjects.Money(v))
         .HasColumnType("DECIMAL(18,2)")
         .HasColumnName("Descuento");

        b.Property(x => x.TarifaEnvio)
         .HasConversion(v => v.Amount, v => new Domain.ValueObjects.Money(v))
         .HasColumnType("DECIMAL(18,2)")
         .HasColumnName("TarifaEnvio");

        b.Property(x => x.Total)
         .HasConversion(v => v.Amount, v => new Domain.ValueObjects.Money(v))
         .HasColumnType("DECIMAL(18,2)")
         .HasColumnName("Total");

        b.Property(x => x.Version).IsConcurrencyToken();

        b.HasOne(x => x.Cliente).WithMany().HasForeignKey(x => x.ClienteId).OnDelete(DeleteBehavior.Restrict);
        b.HasOne(x => x.Pago).WithOne().HasForeignKey<Pago>(p => p.PedidoId).OnDelete(DeleteBehavior.Restrict);
        b.HasOne(x => x.Despacho).WithOne().HasForeignKey<Despacho>(d => d.PedidoId).OnDelete(DeleteBehavior.Restrict);
        b.HasOne(x => x.Devolucion).WithOne().HasForeignKey<Devolucion>(d => d.PedidoId).OnDelete(DeleteBehavior.Restrict);

        b.Ignore(x => x.Detalles);
    }
}