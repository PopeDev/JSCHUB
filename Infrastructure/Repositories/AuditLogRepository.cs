using JSCHUB.Domain.Entities;
using JSCHUB.Domain.Interfaces;
using JSCHUB.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace JSCHUB.Infrastructure.Repositories;

public class AuditLogRepository : IAuditLogRepository
{
    private readonly ReminderDbContext _context;

    public AuditLogRepository(ReminderDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<AuditLog>> GetByEntityAsync(string entityType, Guid entityId, CancellationToken ct = default)
    {
        return await _context.AuditLogs
            .Where(x => x.EntityType == entityType && x.EntityId == entityId)
            .OrderByDescending(x => x.At)
            .ToListAsync(ct);
    }

    public async Task AddAsync(AuditLog log, CancellationToken ct = default)
    {
        await _context.AuditLogs.AddAsync(log, ct);
        await _context.SaveChangesAsync(ct);
    }
}
