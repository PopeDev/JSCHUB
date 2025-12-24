using JSCHUB.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace JSCHUB.Infrastructure.Data.Configurations;

public class SprintTareaHistoricoConfiguration : IEntityTypeConfiguration<SprintTareaHistorico>
{
    public void Configure(EntityTypeBuilder<SprintTareaHistorico> builder)
    {
        builder.ToTable("sprint_tareas_historico");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.TareaId)
            .IsRequired();

        builder.Property(x => x.TareaTitulo)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(x => x.TareaDescripcion)
            .HasMaxLength(2000);

        builder.Property(x => x.AsignadoANombre)
            .HasMaxLength(100);

        builder.Property(x => x.FueEntregada)
            .IsRequired();

        builder.Property(x => x.ColumnaFinal)
            .HasMaxLength(100);

        builder.Property(x => x.EraComprometida)
            .IsRequired();

        builder.Property(x => x.SprintsTranscurridos)
            .IsRequired();

        builder.Property(x => x.FechaRegistro)
            .IsRequired();

        // Relación con Sprint
        builder.HasOne(x => x.Sprint)
            .WithMany(x => x.TareasHistorico)
            .HasForeignKey(x => x.SprintId)
            .OnDelete(DeleteBehavior.Cascade);

        // Índices
        builder.HasIndex(x => x.SprintId);
        builder.HasIndex(x => x.TareaId);
        builder.HasIndex(x => new { x.SprintId, x.FueEntregada });
    }
}
