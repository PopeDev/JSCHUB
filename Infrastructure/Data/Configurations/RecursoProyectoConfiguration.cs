using JSCHUB.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace JSCHUB.Infrastructure.Data.Configurations;

public class RecursoProyectoConfiguration : IEntityTypeConfiguration<RecursoProyecto>
{
    public void Configure(EntityTypeBuilder<RecursoProyecto> builder)
    {
        builder.ToTable("recursos_proyecto");
        builder.HasKey(x => x.Id);

        builder.Property(x => x.Nombre)
            .HasMaxLength(200)
            .IsRequired();

        builder.Property(x => x.Tipo)
            .HasConversion<string>()
            .HasMaxLength(30)
            .IsRequired();

        builder.Property(x => x.Url)
            .HasMaxLength(2000);

        builder.Property(x => x.Contenido)
            .HasMaxLength(10000);

        builder.Property(x => x.Etiquetas)
            .HasMaxLength(500);

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

        // Índices
        builder.HasIndex(x => x.ProyectoId);
        builder.HasIndex(x => x.Tipo);
        builder.HasIndex(x => x.Nombre);
        builder.HasIndex(x => x.ModificadoEl);
    }
}
