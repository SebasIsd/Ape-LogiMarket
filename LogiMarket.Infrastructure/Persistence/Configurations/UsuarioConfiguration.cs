using LogiMarket.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace LogiMarket.Infrastructure.Persistence.Configurations;

public class UsuarioConfiguration : IEntityTypeConfiguration<Usuario>
{
    public void Configure(EntityTypeBuilder<Usuario> b)
    {
        b.ToTable("Usuarios");
        b.HasKey(x => x.Id);
        b.HasIndex(x => x.Username).IsUnique();
        b.Property(x => x.Username).HasMaxLength(40).IsRequired();
        b.Property(x => x.PasswordHash).HasMaxLength(200).IsRequired();
        b.Property(x => x.Rol).HasConversion<string>().HasMaxLength(20);
        b.Property(x => x.Activo);
    }
}