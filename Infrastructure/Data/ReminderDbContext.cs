using JSCHUB.Domain.Entities;
using JSCHUB.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace JSCHUB.Infrastructure.Data;

public class ReminderDbContext : DbContext
{
    public ReminderDbContext(DbContextOptions<ReminderDbContext> options) : base(options)
    {
    }

    public DbSet<ReminderItem> ReminderItems => Set<ReminderItem>();
    public DbSet<Alert> Alerts => Set<Alert>();
    public DbSet<AuditLog> AuditLogs => Set<AuditLog>();
    
    // Módulo de Gastos
    public DbSet<Persona> Personas => Set<Persona>();
    public DbSet<Gasto> Gastos => Set<Gasto>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(ReminderDbContext).Assembly);
    }
}
