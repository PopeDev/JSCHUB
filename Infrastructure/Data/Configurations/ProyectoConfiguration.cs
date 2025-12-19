using JSCHUB.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace JSCHUB.Infrastructure.Data.Configurations;

public class ProyectoConfiguration : IEntityTypeConfiguration<Proyecto>
{
    public void Configure(EntityTypeBuilder<Proyecto> builder)
    {
        builder.ToTable("proyectos");
        builder.HasKey(x => x.Id);

        builder.Property(x => x.Nombre)
            .HasMaxLength(200)
            .IsRequired();

        builder.Property(x => x.Descripcion)
            .HasMaxLength(2000);

        builder.Property(x => x.Estado)
            .HasConversion<string>()
            .HasMaxLength(20)
            .IsRequired();

        builder.Property(x => x.EnlacePrincipal)
            .HasMaxLength(2000);

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

        // Índices para búsquedas frecuentes
        builder.HasIndex(x => x.Nombre);
        builder.HasIndex(x => x.Estado);
        builder.HasIndex(x => x.ModificadoEl);

        // Relaciones
        builder.HasMany(x => x.Enlaces)
            .WithOne(x => x.Proyecto)
            .HasForeignKey(x => x.ProyectoId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(x => x.Recursos)
            .WithOne(x => x.Proyecto)
            .HasForeignKey(x => x.ProyectoId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
