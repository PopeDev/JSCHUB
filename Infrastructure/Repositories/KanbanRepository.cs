using JSCHUB.Domain.Entities;
using JSCHUB.Domain.Interfaces;
using JSCHUB.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace JSCHUB.Infrastructure.Repositories;

public class KanbanRepository : IKanbanRepository
{
    private readonly ReminderDbContext _context;

    public KanbanRepository(ReminderDbContext context)
    {
        _context = context;
    }

    // =====================
    // Columnas
    // =====================

    public async Task<KanbanColumn?> GetColumnByIdAsync(Guid id, CancellationToken ct = default)
    {
        return await _context.KanbanColumnas.FindAsync(new object[] { id }, ct);
    }

    public async Task<KanbanColumn?> GetColumnByIdWithTasksAsync(Guid id, CancellationToken ct = default)
    {
        return await _context.KanbanColumnas
            .Include(c => c.Tareas.OrderBy(t => t.Posicion))
                .ThenInclude(t => t.AsignadoA)
            .FirstOrDefaultAsync(c => c.Id == id, ct);
    }

    public async Task<IEnumerable<KanbanColumn>> GetColumnsByProyectoIdAsync(Guid proyectoId, CancellationToken ct = default)
    {
        return await _context.KanbanColumnas
            .Where(c => c.ProyectoId == proyectoId)
            .OrderBy(c => c.Posicion)
            .ToListAsync(ct);
    }

    public async Task<IEnumerable<KanbanColumn>> GetColumnsByProyectoIdWithTasksAsync(Guid proyectoId, CancellationToken ct = default)
    {
        return await _context.KanbanColumnas
            .Include(c => c.Tareas.OrderBy(t => t.Posicion))
                .ThenInclude(t => t.AsignadoA)
            .Where(c => c.ProyectoId == proyectoId)
            .OrderBy(c => c.Posicion)
            .ToListAsync(ct);
    }

    public async Task<int> CountColumnsByProyectoIdAsync(Guid proyectoId, CancellationToken ct = default)
    {
        return await _context.KanbanColumnas
            .CountAsync(c => c.ProyectoId == proyectoId, ct);
    }

    public async Task<int> GetMaxColumnPositionAsync(Guid proyectoId, CancellationToken ct = default)
    {
        var maxPos = await _context.KanbanColumnas
            .Where(c => c.ProyectoId == proyectoId)
            .MaxAsync(c => (int?)c.Posicion, ct);

        return maxPos ?? -1;
    }

    public async Task<KanbanColumn> AddColumnAsync(KanbanColumn column, CancellationToken ct = default)
    {
        _context.KanbanColumnas.Add(column);
        await _context.SaveChangesAsync(ct);
        return column;
    }

    public async Task UpdateColumnAsync(KanbanColumn column, CancellationToken ct = default)
    {
        _context.Entry(column).State = EntityState.Modified;
        await _context.SaveChangesAsync(ct);
    }

    public async Task DeleteColumnAsync(KanbanColumn column, CancellationToken ct = default)
    {
        _context.KanbanColumnas.Remove(column);
        await _context.SaveChangesAsync(ct);
    }

    public async Task UpdateColumnsPositionsAsync(IEnumerable<KanbanColumn> columns, CancellationToken ct = default)
    {
        foreach (var column in columns)
        {
            _context.Entry(column).Property(c => c.Posicion).IsModified = true;
            _context.Entry(column).Property(c => c.ModificadoPor).IsModified = true;
            _context.Entry(column).Property(c => c.ModificadoEl).IsModified = true;
        }
        await _context.SaveChangesAsync(ct);
    }

    // =====================
    // Tareas
    // =====================

    public async Task<KanbanTask?> GetTaskByIdAsync(Guid id, CancellationToken ct = default)
    {
        return await _context.KanbanTareas
            .Include(t => t.AsignadoA)
            .FirstOrDefaultAsync(t => t.Id == id, ct);
    }

    public async Task<IEnumerable<KanbanTask>> GetTasksByColumnIdAsync(Guid columnId, CancellationToken ct = default)
    {
        return await _context.KanbanTareas
            .Include(t => t.AsignadoA)
            .Where(t => t.ColumnaId == columnId)
            .OrderBy(t => t.Posicion)
            .ToListAsync(ct);
    }

    public async Task<int> CountTasksByColumnIdAsync(Guid columnId, CancellationToken ct = default)
    {
        return await _context.KanbanTareas
            .CountAsync(t => t.ColumnaId == columnId, ct);
    }

    public async Task<int> GetMaxTaskPositionAsync(Guid columnId, CancellationToken ct = default)
    {
        var maxPos = await _context.KanbanTareas
            .Where(t => t.ColumnaId == columnId)
            .MaxAsync(t => (int?)t.Posicion, ct);

        return maxPos ?? -1;
    }

    public async Task<KanbanTask> AddTaskAsync(KanbanTask task, CancellationToken ct = default)
    {
        _context.KanbanTareas.Add(task);
        await _context.SaveChangesAsync(ct);
        return task;
    }

    public async Task UpdateTaskAsync(KanbanTask task, CancellationToken ct = default)
    {
        _context.Entry(task).State = EntityState.Modified;
        await _context.SaveChangesAsync(ct);
    }

    public async Task DeleteTaskAsync(KanbanTask task, CancellationToken ct = default)
    {
        _context.KanbanTareas.Remove(task);
        await _context.SaveChangesAsync(ct);
    }

    public async Task UpdateTasksPositionsAsync(IEnumerable<KanbanTask> tasks, CancellationToken ct = default)
    {
        foreach (var task in tasks)
        {
            _context.Entry(task).Property(t => t.Posicion).IsModified = true;
            _context.Entry(task).Property(t => t.ColumnaId).IsModified = true;
            _context.Entry(task).Property(t => t.ModificadoPor).IsModified = true;
            _context.Entry(task).Property(t => t.ModificadoEl).IsModified = true;
        }
        await _context.SaveChangesAsync(ct);
    }

    public async Task MoveTasksToColumnAsync(Guid sourceColumnId, Guid destColumnId, CancellationToken ct = default)
    {
        var tasks = await _context.KanbanTareas
            .Where(t => t.ColumnaId == sourceColumnId)
            .ToListAsync(ct);

        var maxPos = await GetMaxTaskPositionAsync(destColumnId, ct);

        foreach (var task in tasks)
        {
            maxPos++;
            task.ColumnaId = destColumnId;
            task.Posicion = maxPos;
            task.ModificadoEl = DateTime.UtcNow;
        }

        await _context.SaveChangesAsync(ct);
    }
}
