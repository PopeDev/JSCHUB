using JSCHUB.Domain.Entities;
using JSCHUB.Domain.Enums;
using JSCHUB.Domain.Interfaces;
using JSCHUB.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace JSCHUB.Infrastructure.Repositories;

public class ReminderItemRepository : IReminderItemRepository
{
    private readonly ReminderDbContext _context;
    private Guid? _proyectoGeneralIdCache;

    public ReminderItemRepository(ReminderDbContext context)
    {
        _context = context;
    }

    public async Task<ReminderItem?> GetByIdAsync(Guid id, CancellationToken ct = default)
    {
        return await _context.ReminderItems.FindAsync([id], ct);
    }

    public async Task<ReminderItem?> GetByIdWithRelacionesAsync(Guid id, CancellationToken ct = default)
    {
        return await _context.ReminderItems
            .Include(x => x.Alerts)
            .Include(x => x.AsignadoA)
            .Include(x => x.ReminderItemsProyecto)
                .ThenInclude(rp => rp.Proyecto)
            .FirstOrDefaultAsync(x => x.Id == id, ct);
    }

    public async Task<IEnumerable<ReminderItem>> GetAllAsync(CancellationToken ct = default)
    {
        return await _context.ReminderItems
            .OrderBy(x => x.NextOccurrenceAt)
            .ToListAsync(ct);
    }

    public async Task<IEnumerable<ReminderItem>> GetActiveAsync(CancellationToken ct = default)
    {
        return await _context.ReminderItems
            .Where(x => x.Status == ItemStatus.Active)
            .OrderBy(x => x.NextOccurrenceAt)
            .ToListAsync(ct);
    }

    public async Task<IEnumerable<ReminderItem>> SearchAsync(
        string? searchText = null,
        Category? category = null,
        ItemStatus? status = null,
        string? tag = null,
        Guid? asignadoAId = null,
        Guid? proyectoId = null,
        IEnumerable<Guid>? usuarioProyectoIds = null,
        int skip = 0,
        int take = 50,
        CancellationToken ct = default)
    {
        var query = await BuildQueryAsync(searchText, category, status, tag, asignadoAId, proyectoId, usuarioProyectoIds, ct);

        return await query
            .Include(x => x.Alerts.Where(a => a.State == AlertState.Open || a.State == AlertState.Acknowledged))
            .Include(x => x.AsignadoA)
            .Include(x => x.ReminderItemsProyecto)
                .ThenInclude(rp => rp.Proyecto)
            .OrderBy(x => x.NextOccurrenceAt)
            .Skip(skip)
            .Take(take)
            .ToListAsync(ct);
    }

    public async Task<int> CountAsync(
        string? searchText = null,
        Category? category = null,
        ItemStatus? status = null,
        string? tag = null,
        Guid? asignadoAId = null,
        Guid? proyectoId = null,
        IEnumerable<Guid>? usuarioProyectoIds = null,
        CancellationToken ct = default)
    {
        var query = await BuildQueryAsync(searchText, category, status, tag, asignadoAId, proyectoId, usuarioProyectoIds, ct);
        return await query.CountAsync(ct);
    }

    public async Task AddAsync(ReminderItem item, CancellationToken ct = default)
    {
        await _context.ReminderItems.AddAsync(item, ct);
        await _context.SaveChangesAsync(ct);
    }

    public async Task UpdateAsync(ReminderItem item, CancellationToken ct = default)
    {
        _context.ReminderItems.Update(item);
        await _context.SaveChangesAsync(ct);
    }

    public async Task DeleteAsync(Guid id, CancellationToken ct = default)
    {
        var item = await GetByIdAsync(id, ct);
        if (item != null)
        {
            _context.ReminderItems.Remove(item);
            await _context.SaveChangesAsync(ct);
        }
    }

    public async Task SetProyectosAsync(Guid reminderItemId, IEnumerable<Guid> proyectoIds, CancellationToken ct = default)
    {
        // Eliminar asociaciones existentes
        var existentes = await _context.ReminderItemsProyectos
            .Where(rp => rp.ReminderItemId == reminderItemId)
            .ToListAsync(ct);

        _context.ReminderItemsProyectos.RemoveRange(existentes);

        // Agregar nuevas asociaciones
        var nuevas = proyectoIds.Select(pId => new ReminderItemProyecto
        {
            ReminderItemId = reminderItemId,
            ProyectoId = pId
        });

        await _context.ReminderItemsProyectos.AddRangeAsync(nuevas, ct);
        await _context.SaveChangesAsync(ct);
    }

    public async Task<Guid?> GetProyectoGeneralIdAsync(CancellationToken ct = default)
    {
        if (_proyectoGeneralIdCache.HasValue)
            return _proyectoGeneralIdCache;

        var proyecto = await _context.Proyectos
            .FirstOrDefaultAsync(p => p.EsGeneral, ct);

        _proyectoGeneralIdCache = proyecto?.Id;
        return _proyectoGeneralIdCache;
    }

    private async Task<IQueryable<ReminderItem>> BuildQueryAsync(
        string? searchText,
        Category? category,
        ItemStatus? status,
        string? tag,
        Guid? asignadoAId,
        Guid? proyectoId,
        IEnumerable<Guid>? usuarioProyectoIds,
        CancellationToken ct)
    {
        var query = _context.ReminderItems.AsQueryable();

        if (!string.IsNullOrWhiteSpace(searchText))
        {
            var search = searchText.ToLower();
            query = query.Where(x =>
                x.Title.ToLower().Contains(search) ||
                (x.Description != null && x.Description.ToLower().Contains(search)));
        }

        if (category.HasValue)
        {
            query = query.Where(x => x.Category == category.Value);
        }

        if (status.HasValue)
        {
            query = query.Where(x => x.Status == status.Value);
        }

        if (!string.IsNullOrWhiteSpace(tag))
        {
            query = query.Where(x => x.Tags.Contains(tag));
        }

        if (asignadoAId.HasValue)
        {
            query = query.Where(x => x.AsignadoAId == asignadoAId.Value);
        }

        // Filtrado por proyecto
        if (proyectoId.HasValue)
        {
            var proyectoGeneralId = await GetProyectoGeneralIdAsync(ct);

            if (proyectoGeneralId.HasValue && proyectoId.Value != proyectoGeneralId.Value)
            {
                // Mostrar recordatorios del proyecto seleccionado O del Proyecto General
                query = query.Where(x =>
                    x.ReminderItemsProyecto.Any(rp => rp.ProyectoId == proyectoId.Value) ||
                    x.ReminderItemsProyecto.Any(rp => rp.ProyectoId == proyectoGeneralId.Value));
            }
            else
            {
                // Si es el Proyecto General, mostrar solo recordatorios del Proyecto General
                query = query.Where(x =>
                    x.ReminderItemsProyecto.Any(rp => rp.ProyectoId == proyectoId.Value));
            }
        }
        else if (usuarioProyectoIds != null && usuarioProyectoIds.Any())
        {
            var proyectoIdsList = usuarioProyectoIds.ToList();
            query = query.Where(x =>
                x.ReminderItemsProyecto.Any(rp => proyectoIdsList.Contains(rp.ProyectoId)));
        }

        return query;
    }
}
