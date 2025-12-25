using JSCHUB.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace JSCHUB.Infrastructure.Data.Configurations;

public class CredencialProyectoConfiguration : IEntityTypeConfiguration<CredencialProyecto>
{
    public void Configure(EntityTypeBuilder<CredencialProyecto> builder)
    {
        builder.ToTable("credenciales_proyecto");
        builder.HasKey(x => x.Id);

        builder.Property(x => x.Nombre)
            .HasMaxLength(100)
            .IsRequired();

        builder.Property(x => x.Usuario)
            .HasMaxLength(500)
            .IsRequired();

        builder.Property(x => x.PasswordCifrado)
            .HasMaxLength(4000)
            .IsRequired();

        builder.Property(x => x.Notas)
            .HasMaxLength(2000);

        builder.Property(x => x.Activa)
            .HasDefaultValue(true)
            .IsRequired();

        builder.Property(x => x.UltimoAcceso);

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

        // Relación con EnlaceProyecto (cascade delete)
        builder.HasOne(x => x.EnlaceProyecto)
            .WithMany(e => e.Credenciales)
            .HasForeignKey(x => x.EnlaceProyectoId)
            .OnDelete(DeleteBehavior.Cascade);

        // Índices
        builder.HasIndex(x => x.EnlaceProyectoId);
        builder.HasIndex(x => x.Activa);
        builder.HasIndex(x => new { x.EnlaceProyectoId, x.Nombre });
    }
}
