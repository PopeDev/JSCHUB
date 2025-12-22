using JSCHUB.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace JSCHUB.Infrastructure.Data.Configurations;

public class GastoProyectoConfiguration : IEntityTypeConfiguration<GastoProyecto>
{
    public void Configure(EntityTypeBuilder<GastoProyecto> builder)
    {
        builder.ToTable("gastos_proyectos");

        // Clave primaria compuesta
        builder.HasKey(x => new { x.GastoId, x.ProyectoId });

        // Relaciones
        builder.HasOne(x => x.Gasto)
            .WithMany(x => x.GastosProyecto)
            .HasForeignKey(x => x.GastoId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(x => x.Proyecto)
            .WithMany(x => x.GastosProyecto)
            .HasForeignKey(x => x.ProyectoId)
            .OnDelete(DeleteBehavior.Restrict); // Restrict: no eliminar proyecto si tiene gastos

        // Índices
        builder.HasIndex(x => x.GastoId);
        builder.HasIndex(x => x.ProyectoId);
    }
}
