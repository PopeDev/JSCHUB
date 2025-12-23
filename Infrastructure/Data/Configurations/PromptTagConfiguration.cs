using JSCHUB.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace JSCHUB.Infrastructure.Data.Configurations;

public class PromptTagConfiguration : IEntityTypeConfiguration<PromptTag>
{
    public void Configure(EntityTypeBuilder<PromptTag> builder)
    {
        builder.ToTable("prompt_tags");

        // Clave compuesta
        builder.HasKey(x => new { x.PromptId, x.TagId });

        // Relación con Prompt
        builder.HasOne(x => x.Prompt)
            .WithMany(p => p.PromptTags)
            .HasForeignKey(x => x.PromptId)
            .OnDelete(DeleteBehavior.Cascade);

        // Relación con Tag
        builder.HasOne(x => x.Tag)
            .WithMany(t => t.PromptTags)
            .HasForeignKey(x => x.TagId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
