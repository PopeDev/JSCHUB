using JSCHUB.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace JSCHUB.Infrastructure.Data.Configurations;

public class TagConfiguration : IEntityTypeConfiguration<Tag>
{
    public void Configure(EntityTypeBuilder<Tag> builder)
    {
        builder.ToTable("tags");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Name)
            .HasMaxLength(100)
            .IsRequired();

        builder.Property(x => x.Activo)
            .HasDefaultValue(true);

        builder.Property(x => x.CreadoEl)
            .HasDefaultValueSql("NOW()");

        // Índices
        builder.HasIndex(x => x.Name).IsUnique();
        builder.HasIndex(x => x.Activo);
    }
}
