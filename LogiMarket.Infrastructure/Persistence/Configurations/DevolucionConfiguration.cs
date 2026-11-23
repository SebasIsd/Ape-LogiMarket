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
        b.HasIndex(x => x.PedidoId).IsUnique(); // una sola devolución por pedido
        b.Property(x => x.Motivo).HasConversion<string>().HasMaxLength(20);
        b.OwnsOne(x => x.Reembolso, o => o.Property(p => p.Amount).HasPrecision(18, 2).HasColumnName("Reembolso"));
        b.Property(x => x.FechaSolicitud);
        b.Property(x => x.Aceptada);
    }
}