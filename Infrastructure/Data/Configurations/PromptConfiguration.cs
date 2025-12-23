using JSCHUB.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace JSCHUB.Infrastructure.Data.Configurations;

public class PromptConfiguration : IEntityTypeConfiguration<Prompt>
{
    public void Configure(EntityTypeBuilder<Prompt> builder)
    {
        builder.ToTable("prompts");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Title)
            .HasMaxLength(200)
            .IsRequired();

        builder.Property(x => x.Description)
            .IsRequired();

        builder.Property(x => x.Activo)
            .HasDefaultValue(true);

        builder.Property(x => x.CreadoEl)
            .HasDefaultValueSql("NOW()");

        builder.Property(x => x.ModificadoEl)
            .HasDefaultValueSql("NOW()");

        // Relación con Proyecto (opcional)
        builder.HasOne(x => x.Proyecto)
            .WithMany()
            .HasForeignKey(x => x.ProyectoId)
            .OnDelete(DeleteBehavior.SetNull);

        // Relación con Usuario (obligatorio)
        builder.HasOne(x => x.CreatedByUser)
            .WithMany(u => u.PromptsCreados)
            .HasForeignKey(x => x.CreatedByUserId)
            .OnDelete(DeleteBehavior.Restrict);

        // Relación con Tool (obligatorio)
        builder.HasOne(x => x.Tool)
            .WithMany(t => t.Prompts)
            .HasForeignKey(x => x.ToolId)
            .OnDelete(DeleteBehavior.Restrict);

        // Índices
        builder.HasIndex(x => x.Activo);
        builder.HasIndex(x => x.ToolId);
        builder.HasIndex(x => x.ProyectoId);
        builder.HasIndex(x => x.CreatedByUserId);
        builder.HasIndex(x => x.ModificadoEl);
    }
}
