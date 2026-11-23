using LogiMarket.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace LogiMarket.Infrastructure.Persistence.Configurations;

public class ClienteConfiguration : IEntityTypeConfiguration<Cliente>
{
    public void Configure(EntityTypeBuilder<Cliente> b)
    {
        b.ToTable("Clientes");
        b.HasKey(x => x.Id);
        b.HasIndex(x => x.Identificacion).IsUnique();
        b.Property(x => x.Identificacion).HasMaxLength(20).IsRequired();
        b.Property(x => x.Nombre).HasMaxLength(120).IsRequired();
        b.Property(x => x.Correo).HasMaxLength(120).IsRequired();
        b.Property(x => x.Telefono).HasMaxLength(30);
        b.Property(x => x.Categoria).HasConversion<string>().HasMaxLength(10);
        b.Property(x => x.Activo);
        b.Property(x => x.FechaCreacion);
    }
}