using JSCHUB.Domain.Entities;
using JSCHUB.Domain.Enums;
using JSCHUB.Domain.Interfaces;
using JSCHUB.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace JSCHUB.Infrastructure.Repositories;

public class AlertRepository : IAlertRepository
{
    private readonly ReminderDbContext _context;

    public AlertRepository(ReminderDbContext context)
    {
        _context = context;
    }

    public async Task<Alert?> GetByIdAsync(Guid id, CancellationToken ct = default)
    {
        return await _context.Alerts
            .Include(x => x.ReminderItem)
            .FirstOrDefaultAsync(x => x.Id == id, ct);
    }

    public async Task<IEnumerable<Alert>> GetByReminderItemIdAsync(Guid reminderItemId, CancellationToken ct = default)
    {
        return await _context.Alerts
            .Where(x => x.ReminderItemId == reminderItemId)
            .OrderByDescending(x => x.CreatedAt)
            .ToListAsync(ct);
    }

    public async Task<IEnumerable<Alert>> SearchAsync(
        AlertState? state = null,
        AlertSeverity? severity = null,
        DateTime? fromDate = null,
        DateTime? toDate = null,
        int skip = 0,
        int take = 50,
        CancellationToken ct = default)
    {
        var query = _context.Alerts
            .Include(x => x.ReminderItem)
            .AsQueryable();

        if (state.HasValue)
        {
            query = query.Where(x => x.State == state.Value);
        }

        if (severity.HasValue)
        {
            query = query.Where(x => x.Severity == severity.Value);
        }

        if (fromDate.HasValue)
        {
            query = query.Where(x => x.OccurrenceAt >= fromDate.Value);
        }

        if (toDate.HasValue)
        {
            query = query.Where(x => x.OccurrenceAt <= toDate.Value);
        }

        return await query
            .OrderByDescending(x => x.Severity)
            .ThenBy(x => x.OccurrenceAt)
            .Skip(skip)
            .Take(take)
            .ToListAsync(ct);
    }

    public async Task<int> CountByStateAsync(AlertState state, CancellationToken ct = default)
    {
        return await _context.Alerts.CountAsync(x => x.State == state, ct);
    }

    public async Task<int> CountOverdueAsync(CancellationToken ct = default)
    {
        var now = DateTime.UtcNow;
        return await _context.Alerts
            .CountAsync(x => 
                x.State != AlertState.Resolved && 
                x.OccurrenceAt < now, ct);
    }

    public async Task<int> CountDueTodayAsync(CancellationToken ct = default)
    {
        var today = DateTime.UtcNow.Date;
        var tomorrow = today.AddDays(1);
        return await _context.Alerts
            .CountAsync(x => 
                x.State != AlertState.Resolved && 
                x.OccurrenceAt >= today && 
                x.OccurrenceAt < tomorrow, ct);
    }

    public async Task<int> CountDueInDaysAsync(int days, CancellationToken ct = default)
    {
        var now = DateTime.UtcNow;
        var endDate = now.AddDays(days);
        return await _context.Alerts
            .CountAsync(x => 
                x.State != AlertState.Resolved && 
                x.OccurrenceAt >= now && 
                x.OccurrenceAt <= endDate, ct);
    }

    public async Task<Alert?> FindExistingAsync(Guid reminderItemId, DateTime occurrenceAt, CancellationToken ct = default)
    {
        return await _context.Alerts
            .FirstOrDefaultAsync(x => 
                x.ReminderItemId == reminderItemId && 
                x.OccurrenceAt == occurrenceAt, ct);
    }

    public async Task AddAsync(Alert alert, CancellationToken ct = default)
    {
        await _context.Alerts.AddAsync(alert, ct);
        await _context.SaveChangesAsync(ct);
    }

    public async Task UpdateAsync(Alert alert, CancellationToken ct = default)
    {
        _context.Alerts.Update(alert);
        await _context.SaveChangesAsync(ct);
    }
}
