using JSCHUB.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace JSCHUB.Infrastructure.Data.Configurations;

public class KanbanColumnConfiguration : IEntityTypeConfiguration<KanbanColumn>
{
    public void Configure(EntityTypeBuilder<KanbanColumn> builder)
    {
        builder.ToTable("kanban_columnas");
        builder.HasKey(x => x.Id);

        builder.Property(x => x.Titulo)
            .HasMaxLength(100)
            .IsRequired();

        builder.Property(x => x.Posicion)
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
        builder.HasIndex(x => new { x.ProyectoId, x.Posicion });

        // Relación con Proyecto
        builder.HasOne(x => x.Proyecto)
            .WithMany()
            .HasForeignKey(x => x.ProyectoId)
            .OnDelete(DeleteBehavior.Cascade);

        // Relación con Tareas
        builder.HasMany(x => x.Tareas)
            .WithOne(x => x.Columna)
            .HasForeignKey(x => x.ColumnaId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
