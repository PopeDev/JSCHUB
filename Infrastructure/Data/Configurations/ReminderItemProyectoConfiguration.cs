using JSCHUB.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace JSCHUB.Infrastructure.Data.Configurations;

public class ReminderItemProyectoConfiguration : IEntityTypeConfiguration<ReminderItemProyecto>
{
    public void Configure(EntityTypeBuilder<ReminderItemProyecto> builder)
    {
        builder.ToTable("reminder_items_proyectos");

        // Clave primaria compuesta
        builder.HasKey(x => new { x.ReminderItemId, x.ProyectoId });

        // Relaciones
        builder.HasOne(x => x.ReminderItem)
            .WithMany(x => x.ReminderItemsProyecto)
            .HasForeignKey(x => x.ReminderItemId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(x => x.Proyecto)
            .WithMany(x => x.ReminderItemsProyecto)
            .HasForeignKey(x => x.ProyectoId)
            .OnDelete(DeleteBehavior.Restrict); // Restrict: no eliminar proyecto si tiene recordatorios

        // Índices
        builder.HasIndex(x => x.ReminderItemId);
        builder.HasIndex(x => x.ProyectoId);
    }
}
