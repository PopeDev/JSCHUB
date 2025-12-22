using JSCHUB.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace JSCHUB.Infrastructure.Data.Configurations;

public class UsuarioProyectoConfiguration : IEntityTypeConfiguration<UsuarioProyecto>
{
    public void Configure(EntityTypeBuilder<UsuarioProyecto> builder)
    {
        builder.ToTable("usuarios_proyectos");

        // Clave primaria compuesta
        builder.HasKey(x => new { x.UsuarioId, x.ProyectoId });

        builder.Property(x => x.Rol)
            .HasConversion<string>()
            .HasMaxLength(20)
            .IsRequired();

        builder.Property(x => x.FechaAsignacion)
            .IsRequired();

        builder.Property(x => x.AsignadoPor)
            .HasMaxLength(100)
            .IsRequired();

        // Relaciones
        builder.HasOne(x => x.Usuario)
            .WithMany(x => x.UsuarioProyectos)
            .HasForeignKey(x => x.UsuarioId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(x => x.Proyecto)
            .WithMany(x => x.UsuariosProyecto)
            .HasForeignKey(x => x.ProyectoId)
            .OnDelete(DeleteBehavior.Cascade);

        // Índices
        builder.HasIndex(x => x.UsuarioId);
        builder.HasIndex(x => x.ProyectoId);
        builder.HasIndex(x => x.Rol);
    }
}
