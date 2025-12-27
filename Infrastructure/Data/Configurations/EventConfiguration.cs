using JSCHUB.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace JSCHUB.Infrastructure.Data.Configurations;

public class EventConfiguration : IEntityTypeConfiguration<Event>
{
    public void Configure(EntityTypeBuilder<Event> builder)
    {
        builder.ToTable("events");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Title)
            .HasMaxLength(200)
            .IsRequired();

        builder.Property(x => x.StartUtc)
            .IsRequired();

        builder.Property(x => x.EndUtc)
            .IsRequired();

        builder.Property(x => x.MeetingUrl)
            .HasMaxLength(500);

        builder.Property(x => x.NotifiedAtUtc);

        // Auditoría
        builder.Property(x => x.CreadoPor)
            .HasMaxLength(100)
            .IsRequired();

        builder.Property(x => x.CreadoEl)
            .IsRequired();

        builder.Property(x => x.ModificadoPor)
            .HasMaxLength(100)
            .IsRequired();

        builder.Property(x => x.ModificadoEl)
            .IsRequired();

        // Índices para consultas frecuentes
        builder.HasIndex(x => x.StartUtc)
            .HasDatabaseName("IX_events_StartUtc");

        builder.HasIndex(x => x.EndUtc)
            .HasDatabaseName("IX_events_EndUtc");

        // Índice compuesto para búsqueda de notificaciones pendientes
        builder.HasIndex(x => new { x.StartUtc, x.NotifiedAtUtc })
            .HasDatabaseName("IX_events_StartUtc_NotifiedAtUtc");
    }
}
