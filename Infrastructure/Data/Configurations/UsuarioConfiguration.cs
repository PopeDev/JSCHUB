using JSCHUB.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace JSCHUB.Infrastructure.Data.Configurations;

public class UsuarioConfiguration : IEntityTypeConfiguration<Usuario>
{
    public void Configure(EntityTypeBuilder<Usuario> builder)
    {
        builder.ToTable("usuarios");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Nombre)
            .HasMaxLength(100)
            .IsRequired();

        builder.Property(x => x.Email)
            .HasMaxLength(150);

        builder.Property(x => x.Telefono)
            .HasMaxLength(20);

        builder.Property(x => x.Activo)
            .HasDefaultValue(true);

        // Índices
        builder.HasIndex(x => x.Nombre).IsUnique();
        builder.HasIndex(x => x.Activo);
    }
}
