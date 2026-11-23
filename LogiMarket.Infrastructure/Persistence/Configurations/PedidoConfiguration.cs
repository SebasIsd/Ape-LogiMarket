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
        b.OwnsOne(x => x.Subtotal, o => o.Property(p => p.Amount).HasPrecision(18, 2).HasColumnName("Subtotal"));
        b.OwnsOne(x => x.Descuento, o => o.Property(p => p.Amount).HasPrecision(18, 2).HasColumnName("Descuento"));
        b.OwnsOne(x => x.TarifaEnvio, o => o.Property(p => p.Amount).HasPrecision(18, 2).HasColumnName("TarifaEnvio"));
        b.OwnsOne(x => x.Total, o => o.Property(p => p.Amount).HasPrecision(18, 2).HasColumnName("Total"));
        b.Property(x => x.Version).IsConcurrencyToken();

        b.HasOne(x => x.Cliente).WithMany().HasForeignKey(x => x.ClienteId).OnDelete(DeleteBehavior.Restrict);
        b.HasOne(x => x.Pago).WithOne().HasForeignKey<Pago>(p => p.PedidoId).OnDelete(DeleteBehavior.Restrict);
        b.HasOne(x => x.Despacho).WithOne().HasForeignKey<Despacho>(d => d.PedidoId).OnDelete(DeleteBehavior.Restrict);
        b.HasOne(x => x.Devolucion).WithOne().HasForeignKey<Devolucion>(d => d.PedidoId).OnDelete(DeleteBehavior.Restrict);

        b.Ignore(x => x.Detalles); // se mapea aparte
        //b.Ignore(p => p.EstadoActual); // en realidad sí lo persistimos, quitar este Ignore
    }
}