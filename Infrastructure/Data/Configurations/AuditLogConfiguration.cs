using JSCHUB.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace JSCHUB.Infrastructure.Data.Configurations;

public class AuditLogConfiguration : IEntityTypeConfiguration<AuditLog>
{
    public void Configure(EntityTypeBuilder<AuditLog> builder)
    {
        builder.ToTable("audit_logs");
        
        builder.HasKey(x => x.Id);
        
        builder.Property(x => x.EntityType)
            .HasMaxLength(100)
            .IsRequired();
        
        builder.Property(x => x.Action)
            .HasMaxLength(50)
            .IsRequired();
        
        builder.Property(x => x.Changes)
            .HasColumnType("jsonb");
        
        builder.Property(x => x.User)
            .HasMaxLength(100);

        // Indexes
        builder.HasIndex(x => new { x.EntityType, x.EntityId });
        builder.HasIndex(x => x.At);
    }
}
