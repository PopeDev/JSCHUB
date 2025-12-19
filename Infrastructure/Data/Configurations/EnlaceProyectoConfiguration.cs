using JSCHUB.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace JSCHUB.Infrastructure.Data.Configurations;

public class EnlaceProyectoConfiguration : IEntityTypeConfiguration<EnlaceProyecto>
{
    public void Configure(EntityTypeBuilder<EnlaceProyecto> builder)
    {
        builder.ToTable("enlaces_proyecto");
        builder.HasKey(x => x.Id);

        builder.Property(x => x.Titulo)
            .HasMaxLength(100)
            .IsRequired();

        builder.Property(x => x.Url)
            .HasMaxLength(2000)
            .IsRequired();

        builder.Property(x => x.Descripcion)
            .HasMaxLength(500);

        builder.Property(x => x.Tipo)
            .HasConversion<string>()
            .HasMaxLength(30)
            .IsRequired();

        builder.Property(x => x.Orden)
            .HasDefaultValue(0)
            .IsRequired();

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
        builder.HasIndex(x => new { x.ProyectoId, x.Orden });
    }
}
