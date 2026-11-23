using LogiMarket.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace LogiMarket.Infrastructure.Persistence.Configurations;

public class AuditoriaConfiguration : IEntityTypeConfiguration<Auditoria>
{
    public void Configure(EntityTypeBuilder<Auditoria> b)
    {
        b.ToTable("Auditorias");
        b.HasKey(x => x.Id);
        b.Property(x => x.Usuario).HasMaxLength(60);
        b.Property(x => x.Accion).HasMaxLength(80);
        b.Property(x => x.Detalle);
        b.Property(x => x.Fecha);
    }
}