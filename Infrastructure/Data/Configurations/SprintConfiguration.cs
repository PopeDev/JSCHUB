using JSCHUB.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace JSCHUB.Infrastructure.Data.Configurations;

public class SprintConfiguration : IEntityTypeConfiguration<Sprint>
{
    public void Configure(EntityTypeBuilder<Sprint> builder)
    {
        builder.ToTable("sprints");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Nombre)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(x => x.Temporizacion)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(x => x.Objetivo)
            .HasMaxLength(1000);

        builder.Property(x => x.FechaInicio)
            .IsRequired();

        builder.Property(x => x.FechaFin)
            .IsRequired();

        builder.Property(x => x.Estado)
            .IsRequired()
            .HasConversion<string>()
            .HasMaxLength(20);

        builder.Property(x => x.TareasComprometidas);
        builder.Property(x => x.TareasEntregadas);
        builder.Property(x => x.PorcentajeCompletitud)
            .HasPrecision(5, 2);
        builder.Property(x => x.FechaCierre);

        builder.Property(x => x.CreadoPor)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(x => x.CreadoEl)
            .IsRequired();

        builder.Property(x => x.ModificadoPor)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(x => x.ModificadoEl)
            .IsRequired();

        // Ignorar propiedad calculada
        builder.Ignore(x => x.DuracionDias);

        // Relación con Proyecto
        builder.HasOne(x => x.Proyecto)
            .WithMany(x => x.Sprints)
            .HasForeignKey(x => x.ProyectoId)
            .OnDelete(DeleteBehavior.Cascade);

        // Índices
        builder.HasIndex(x => x.ProyectoId);
        builder.HasIndex(x => x.Estado);
        builder.HasIndex(x => new { x.ProyectoId, x.Estado });
    }
}
