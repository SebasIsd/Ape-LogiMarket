using LogiMarket.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace LogiMarket.Infrastructure.Persistence.Configurations;

public class PedidoDetalleConfiguration : IEntityTypeConfiguration<PedidoDetalle>
{
    public void Configure(EntityTypeBuilder<PedidoDetalle> b)
    {
        b.ToTable("PedidoDetalles");
        b.HasKey(x => x.Id);
        b.Property(x => x.Sku).HasMaxLength(30).IsRequired();
        b.Property(x => x.Nombre).HasMaxLength(120).IsRequired();
        b.Property(x => x.EsKit);
        b.Property(x => x.Cantidad);
        b.OwnsOne(x => x.PrecioUnitarioInstantanea,
            o => o.Property(p => p.Amount).HasPrecision(18, 2).HasColumnName("PrecioUnitario"));

        b.HasOne<Pedido>().WithMany().HasForeignKey(x => x.PedidoId).OnDelete(DeleteBehavior.Cascade);
    }
}