using JSCHUB.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace JSCHUB.Infrastructure.Data.Configurations;

public class AlertConfiguration : IEntityTypeConfiguration<Alert>
{
    public void Configure(EntityTypeBuilder<Alert> builder)
    {
        builder.ToTable("alerts");
        
        builder.HasKey(x => x.Id);
        
        builder.Property(x => x.State)
            .HasConversion<string>()
            .HasMaxLength(50);
        
        builder.Property(x => x.Severity)
            .HasConversion<string>()
            .HasMaxLength(50);
        
        builder.Property(x => x.Message)
            .HasMaxLength(500);

        // Indexes
        builder.HasIndex(x => x.State);
        builder.HasIndex(x => x.Severity);
        builder.HasIndex(x => x.TriggerAt);
        builder.HasIndex(x => x.ReminderItemId);
        builder.HasIndex(x => new { x.ReminderItemId, x.OccurrenceAt });
    }
}
