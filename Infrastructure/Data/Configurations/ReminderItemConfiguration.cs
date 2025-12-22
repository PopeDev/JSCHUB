using System.Text.Json;
using JSCHUB.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace JSCHUB.Infrastructure.Data.Configurations;

public class ReminderItemConfiguration : IEntityTypeConfiguration<ReminderItem>
{
    public void Configure(EntityTypeBuilder<ReminderItem> builder)
    {
        builder.ToTable("reminder_items");
        
        builder.HasKey(x => x.Id);
        
        builder.Property(x => x.Title)
            .HasMaxLength(200)
            .IsRequired();
        
        builder.Property(x => x.Description)
            .HasMaxLength(2000);
        
        builder.Property(x => x.Category)
            .HasConversion<string>()
            .HasMaxLength(50);
        
        builder.Property(x => x.Status)
            .HasConversion<string>()
            .HasMaxLength(50);
        
        builder.Property(x => x.ScheduleType)
            .HasConversion<string>()
            .HasMaxLength(50);
        
        builder.Property(x => x.RecurrenceFrequency)
            .HasConversion<string>()
            .HasMaxLength(50);
        
        // AsignadoAId se configura en la relación
        
        builder.Property(x => x.Timezone)
            .HasMaxLength(100)
            .HasDefaultValue("Europe/Madrid");
        
        builder.Property(x => x.CreatedBy)
            .HasMaxLength(100);
        
        builder.Property(x => x.UpdatedBy)
            .HasMaxLength(100);

        // JSON columns for collections
        builder.Property(x => x.Tags)
            .HasColumnType("jsonb")
            .HasConversion(
                v => JsonSerializer.Serialize(v, (JsonSerializerOptions?)null),
                v => JsonSerializer.Deserialize<List<string>>(v, (JsonSerializerOptions?)null) ?? new List<string>(),
                new ValueComparer<List<string>>(
                    (c1, c2) => c1!.SequenceEqual(c2!),
                    c => c.Aggregate(0, (a, v) => HashCode.Combine(a, v.GetHashCode())),
                    c => c.ToList()));

        builder.Property(x => x.LeadTimeDays)
            .HasColumnType("jsonb")
            .HasConversion(
                v => JsonSerializer.Serialize(v, (JsonSerializerOptions?)null),
                v => JsonSerializer.Deserialize<List<int>>(v, (JsonSerializerOptions?)null) ?? new List<int> { 30 },
                new ValueComparer<List<int>>(
                    (c1, c2) => c1!.SequenceEqual(c2!),
                    c => c.Aggregate(0, (a, v) => HashCode.Combine(a, v.GetHashCode())),
                    c => c.ToList()));

        builder.Property(x => x.Metadata)
            .HasColumnType("jsonb")
            .HasConversion(
                v => JsonSerializer.Serialize(v, (JsonSerializerOptions?)null),
                v => JsonSerializer.Deserialize<Dictionary<string, string>>(v, (JsonSerializerOptions?)null) ?? new Dictionary<string, string>(),
                new ValueComparer<Dictionary<string, string>>(
                    (c1, c2) => c1!.SequenceEqual(c2!),
                    c => c.Aggregate(0, (a, v) => HashCode.Combine(a, v.GetHashCode())),
                    c => c.ToDictionary(x => x.Key, x => x.Value)));

        // Indexes
        builder.HasIndex(x => x.Status);
        builder.HasIndex(x => x.Category);
        builder.HasIndex(x => x.NextOccurrenceAt);
        builder.HasIndex(x => x.AsignadoAId);

        // Relación con Alerts
        builder.HasMany(x => x.Alerts)
            .WithOne(x => x.ReminderItem)
            .HasForeignKey(x => x.ReminderItemId)
            .OnDelete(DeleteBehavior.Cascade);

        // Relación con Usuario (AsignadoA)
        builder.HasOne(x => x.AsignadoA)
            .WithMany(x => x.ReminderItemsAsignados)
            .HasForeignKey(x => x.AsignadoAId)
            .OnDelete(DeleteBehavior.SetNull); // Si se elimina el usuario, se quita la asignación
    }
}
