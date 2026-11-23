using LogiMarket.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace LogiMarket.Infrastructure.Persistence.Configurations;

public class EventoOutboxConfiguration : IEntityTypeConfiguration<EventoOutbox>
{
    public void Configure(EntityTypeBuilder<EventoOutbox> b)
    {
        b.ToTable("EventosOutbox");
        b.HasKey(x => x.Id);
        b.Property(x => x.TipoEvento).HasMaxLength(60).IsRequired();
        b.Property(x => x.Payload).IsRequired();
        b.Property(x => x.CreadoEn);
        b.Property(x => x.Procesado);
        b.Property(x => x.ProcesadoEn);
        b.Property(x => x.Reintentos);
    }
}