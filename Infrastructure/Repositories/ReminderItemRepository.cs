using JSCHUB.Domain.Entities;
using JSCHUB.Domain.Enums;
using JSCHUB.Domain.Interfaces;
using JSCHUB.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace JSCHUB.Infrastructure.Repositories;

public class ReminderItemRepository : IReminderItemRepository
{
    private readonly ReminderDbContext _context;

    public ReminderItemRepository(ReminderDbContext context)
    {
        _context = context;
    }

    public async Task<ReminderItem?> GetByIdAsync(Guid id, CancellationToken ct = default)
    {
        return await _context.ReminderItems.FindAsync([id], ct);
    }

    public async Task<ReminderItem?> GetByIdWithAlertsAsync(Guid id, CancellationToken ct = default)
    {
        return await _context.ReminderItems
            .Include(x => x.Alerts)
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
        string? assignee = null,
        int skip = 0,
        int take = 50,
        CancellationToken ct = default)
    {
        var query = _context.ReminderItems
            .Include(x => x.Alerts.Where(a => a.State == AlertState.Open || a.State == AlertState.Acknowledged))
            .AsQueryable();

        query = ApplyFilters(query, searchText, category, status, tag, assignee);

        return await query
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
        string? assignee = null,
        CancellationToken ct = default)
    {
        var query = _context.ReminderItems.AsQueryable();
        query = ApplyFilters(query, searchText, category, status, tag, assignee);
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

    private static IQueryable<ReminderItem> ApplyFilters(
        IQueryable<ReminderItem> query,
        string? searchText,
        Category? category,
        ItemStatus? status,
        string? tag,
        string? assignee)
    {
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

        if (!string.IsNullOrWhiteSpace(assignee))
        {
            query = query.Where(x => x.Assignee == assignee);
        }

        return query;
    }
}
