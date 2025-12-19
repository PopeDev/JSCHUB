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

    // Módulo de Proyectos
    public DbSet<Proyecto> Proyectos => Set<Proyecto>();
    public DbSet<EnlaceProyecto> EnlacesProyecto => Set<EnlaceProyecto>();
    public DbSet<RecursoProyecto> RecursosProyecto => Set<RecursoProyecto>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(ReminderDbContext).Assembly);
    }
}
