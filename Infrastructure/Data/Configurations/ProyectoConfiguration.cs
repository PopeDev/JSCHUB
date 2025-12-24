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

        builder.Property(x => x.EsGeneral)
            .HasDefaultValue(false)
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

        // Índices para búsquedas frecuentes
        builder.HasIndex(x => x.Nombre);
        builder.HasIndex(x => x.Estado);
        builder.HasIndex(x => x.ModificadoEl);
        builder.HasIndex(x => x.EsGeneral);

        // Índice único filtrado para garantizar solo un Proyecto General
        // Nota: En PostgreSQL esto se logra con un índice único parcial
        builder.HasIndex(x => x.EsGeneral)
            .HasFilter("\"EsGeneral\" = true")
            .IsUnique()
            .HasDatabaseName("IX_proyectos_EsGeneral_Unique");

        // Relaciones
        builder.HasMany(x => x.Enlaces)
            .WithOne(x => x.Proyecto)
            .HasForeignKey(x => x.ProyectoId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(x => x.Recursos)
            .WithOne(x => x.Proyecto)
            .HasForeignKey(x => x.ProyectoId)
            .OnDelete(DeleteBehavior.Cascade);

        // === Sprint Activo ===
        builder.Property(x => x.SprintActivoId);

        builder.HasOne(x => x.SprintActivo)
            .WithOne()
            .HasForeignKey<Proyecto>(x => x.SprintActivoId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.HasIndex(x => x.SprintActivoId);
    }
}
