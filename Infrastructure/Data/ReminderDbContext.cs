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
    public DbSet<Usuario> Usuarios => Set<Usuario>();
    public DbSet<Gasto> Gastos => Set<Gasto>();

    // Módulo de Proyectos
    public DbSet<Proyecto> Proyectos => Set<Proyecto>();
    public DbSet<EnlaceProyecto> EnlacesProyecto => Set<EnlaceProyecto>();
    public DbSet<RecursoProyecto> RecursosProyecto => Set<RecursoProyecto>();
    public DbSet<CredencialProyecto> CredencialesProyecto => Set<CredencialProyecto>();

    // Módulo Kanban
    public DbSet<KanbanColumn> KanbanColumnas => Set<KanbanColumn>();
    public DbSet<KanbanTask> KanbanTareas => Set<KanbanTask>();

    // Relaciones N:M
    public DbSet<UsuarioProyecto> UsuariosProyectos => Set<UsuarioProyecto>();
    public DbSet<GastoProyecto> GastosProyectos => Set<GastoProyecto>();
    public DbSet<ReminderItemProyecto> ReminderItemsProyectos => Set<ReminderItemProyecto>();

    // Módulo de Prompts
    public DbSet<Tool> Tools => Set<Tool>();
    public DbSet<Tag> Tags => Set<Tag>();
    public DbSet<Prompt> Prompts => Set<Prompt>();
    public DbSet<PromptTag> PromptTags => Set<PromptTag>();

    // Módulo de Sprints
    public DbSet<Sprint> Sprints => Set<Sprint>();
    public DbSet<SprintTareaHistorico> SprintTareasHistorico => Set<SprintTareaHistorico>();

    // Módulo de Calendario
    public DbSet<Event> Events => Set<Event>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(ReminderDbContext).Assembly);
    }
}
