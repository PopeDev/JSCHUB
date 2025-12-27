using JSCHUB.Domain.Entities;
using JSCHUB.Domain.Interfaces;
using JSCHUB.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace JSCHUB.Infrastructure.Repositories;

public class EventRepository : IEventRepository
{
    private readonly ReminderDbContext _context;

    public EventRepository(ReminderDbContext context) => _context = context;

    public async Task<IEnumerable<Event>> GetByRangeAsync(DateTime fromUtc, DateTime toUtc, CancellationToken ct = default)
    {
        return await _context.Events
            .Where(e => e.StartUtc >= fromUtc && e.StartUtc < toUtc)
            .OrderBy(e => e.StartUtc)
            .ToListAsync(ct);
    }

    public async Task<Event?> GetByIdAsync(Guid id, CancellationToken ct = default)
    {
        return await _context.Events.FindAsync(new object[] { id }, ct);
    }

    public async Task<Event> AddAsync(Event evento, CancellationToken ct = default)
    {
        await _context.Events.AddAsync(evento, ct);
        await _context.SaveChangesAsync(ct);
        return evento;
    }

    public async Task UpdateAsync(Event evento, CancellationToken ct = default)
    {
        _context.Events.Update(evento);
        await _context.SaveChangesAsync(ct);
    }

    public async Task DeleteAsync(Guid id, CancellationToken ct = default)
    {
        var evento = await _context.Events.FindAsync(new object[] { id }, ct);
        if (evento is not null)
        {
            _context.Events.Remove(evento);
            await _context.SaveChangesAsync(ct);
        }
    }

    public async Task<IEnumerable<Event>> GetPendingNotificationAsync(DateTime fromUtc, DateTime toUtc, CancellationToken ct = default)
    {
        return await _context.Events
            .Where(e => e.StartUtc >= fromUtc
                     && e.StartUtc <= toUtc
                     && e.NotifiedAtUtc == null)
            .OrderBy(e => e.StartUtc)
            .ToListAsync(ct);
    }

    public async Task MarkAsNotifiedAsync(IEnumerable<Guid> eventIds, CancellationToken ct = default)
    {
        var ids = eventIds.ToList();
        if (ids.Count == 0) return;

        var now = DateTime.UtcNow;
        await _context.Events
            .Where(e => ids.Contains(e.Id))
            .ExecuteUpdateAsync(s => s.SetProperty(e => e.NotifiedAtUtc, now), ct);
    }
}
