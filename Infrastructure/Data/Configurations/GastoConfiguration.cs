using JSCHUB.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace JSCHUB.Infrastructure.Data.Configurations;

public class GastoConfiguration : IEntityTypeConfiguration<Gasto>
{
    public void Configure(EntityTypeBuilder<Gasto> builder)
    {
        builder.ToTable("gastos");
        
        builder.HasKey(x => x.Id);
        
        builder.Property(x => x.Concepto)
            .HasMaxLength(200)
            .IsRequired();
        
        builder.Property(x => x.Notas)
            .HasMaxLength(1000);
        
        builder.Property(x => x.Importe)
            .HasPrecision(18, 2)
            .IsRequired();
        
        builder.Property(x => x.Moneda)
            .HasMaxLength(3)
            .HasDefaultValue("EUR")
            .IsRequired();
        
        builder.Property(x => x.FechaPago)
            .IsRequired();
        
        builder.Property(x => x.HoraPago)
            .IsRequired();
        
        builder.Property(x => x.Estado)
            .HasConversion<string>()
            .HasMaxLength(30);
        
        // Relación con Usuario
        builder.HasOne(x => x.PagadoPor)
            .WithMany(x => x.Gastos)
            .HasForeignKey(x => x.PagadoPorId)
            .OnDelete(DeleteBehavior.Restrict);
        
        // Índices
        builder.HasIndex(x => x.FechaPago);
        builder.HasIndex(x => x.Estado);
        builder.HasIndex(x => x.PagadoPorId);
        builder.HasIndex(x => x.Concepto);
    }
}
