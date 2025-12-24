using JSCHUB.Domain.Entities;
using JSCHUB.Domain.Enums;
using JSCHUB.Domain.Interfaces;
using JSCHUB.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace JSCHUB.Infrastructure.Repositories;

public class SprintRepository : ISprintRepository
{
    private readonly ReminderDbContext _context;

    public SprintRepository(ReminderDbContext context)
    {
        _context = context;
    }

    public async Task<Sprint?> GetByIdAsync(Guid id, CancellationToken ct = default)
    {
        return await _context.Sprints.FindAsync([id], ct);
    }

    public async Task<Sprint?> GetByIdWithDetailsAsync(Guid id, CancellationToken ct = default)
    {
        return await _context.Sprints
            .Include(x => x.Proyecto)
            .Include(x => x.Tareas)
                .ThenInclude(t => t.Columna)
            .Include(x => x.Tareas)
                .ThenInclude(t => t.AsignadoA)
            .FirstOrDefaultAsync(x => x.Id == id, ct);
    }

    public async Task<IEnumerable<Sprint>> GetByProyectoIdAsync(Guid proyectoId, CancellationToken ct = default)
    {
        return await _context.Sprints
            .Where(x => x.ProyectoId == proyectoId)
            .OrderByDescending(x => x.FechaInicio)
            .ToListAsync(ct);
    }

    public async Task<Sprint?> GetSprintActivoAsync(Guid proyectoId, CancellationToken ct = default)
    {
        return await _context.Sprints
            .Include(x => x.Tareas)
                .ThenInclude(t => t.Columna)
            .FirstOrDefaultAsync(x => x.ProyectoId == proyectoId && x.Estado == EstadoSprint.Activo, ct);
    }

    public async Task<IEnumerable<Sprint>> GetByEstadoAsync(Guid proyectoId, EstadoSprint estado, CancellationToken ct = default)
    {
        return await _context.Sprints
            .Where(x => x.ProyectoId == proyectoId && x.Estado == estado)
            .OrderByDescending(x => x.FechaInicio)
            .ToListAsync(ct);
    }

    public async Task<Sprint> AddAsync(Sprint sprint, CancellationToken ct = default)
    {
        await _context.Sprints.AddAsync(sprint, ct);
        await _context.SaveChangesAsync(ct);
        return sprint;
    }

    public async Task UpdateAsync(Sprint sprint, CancellationToken ct = default)
    {
        _context.Sprints.Update(sprint);
        await _context.SaveChangesAsync(ct);
    }

    public async Task DeleteAsync(Sprint sprint, CancellationToken ct = default)
    {
        _context.Sprints.Remove(sprint);
        await _context.SaveChangesAsync(ct);
    }

    public async Task<IEnumerable<SprintTareaHistorico>> GetHistoricoAsync(Guid sprintId, CancellationToken ct = default)
    {
        return await _context.SprintTareasHistorico
            .Where(x => x.SprintId == sprintId)
            .OrderBy(x => x.FueEntregada)
            .ThenBy(x => x.TareaTitulo)
            .ToListAsync(ct);
    }

    public async Task AddHistoricoAsync(SprintTareaHistorico historico, CancellationToken ct = default)
    {
        await _context.SprintTareasHistorico.AddAsync(historico, ct);
        await _context.SaveChangesAsync(ct);
    }

    public async Task AddHistoricoRangeAsync(IEnumerable<SprintTareaHistorico> historicos, CancellationToken ct = default)
    {
        await _context.SprintTareasHistorico.AddRangeAsync(historicos, ct);
        await _context.SaveChangesAsync(ct);
    }
}
