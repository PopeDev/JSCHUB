using JSCHUB.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace JSCHUB.Infrastructure.Data.Configurations;

public class KanbanTaskConfiguration : IEntityTypeConfiguration<KanbanTask>
{
    public void Configure(EntityTypeBuilder<KanbanTask> builder)
    {
        builder.ToTable("kanban_tareas");
        builder.HasKey(x => x.Id);

        builder.Property(x => x.Titulo)
            .HasMaxLength(200)
            .IsRequired();

        builder.Property(x => x.Descripcion)
            .HasMaxLength(2000);

        builder.Property(x => x.Prioridad)
            .HasConversion<string>()
            .HasMaxLength(20)
            .IsRequired();

        builder.Property(x => x.HorasEstimadas)
            .HasPrecision(8, 2)
            .HasDefaultValue(0m)
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

        // Relación con Proyecto
        builder.HasOne(x => x.Proyecto)
            .WithMany()
            .HasForeignKey(x => x.ProyectoId)
            .OnDelete(DeleteBehavior.Cascade);

        // Relación con Columna (configurada en KanbanColumnConfiguration)
        // Relación con Usuario asignado
        builder.HasOne(x => x.AsignadoA)
            .WithMany()
            .HasForeignKey(x => x.AsignadoAId)
            .OnDelete(DeleteBehavior.SetNull);

        // Índices
        builder.HasIndex(x => x.ProyectoId);
        builder.HasIndex(x => x.ColumnaId);
        builder.HasIndex(x => x.AsignadoAId);
        builder.HasIndex(x => new { x.ColumnaId, x.Posicion });
        builder.HasIndex(x => x.Prioridad);
    }
}
