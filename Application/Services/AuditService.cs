using JSCHUB.Application.Interfaces;
using JSCHUB.Domain.Entities;
using JSCHUB.Domain.Interfaces;

namespace JSCHUB.Application.Services;

public class AuditService : IAuditService
{
    private readonly IAuditLogRepository _repository;

    public AuditService(IAuditLogRepository repository)
    {
        _repository = repository;
    }

    public async Task<IEnumerable<AuditLog>> GetByEntityAsync(string entityType, Guid entityId, CancellationToken ct = default)
    {
        return await _repository.GetByEntityAsync(entityType, entityId, ct);
    }

    public async Task LogAsync(string entityType, Guid entityId, string action, string? changes = null, CancellationToken ct = default)
    {
        var log = AuditLog.Create(entityType, entityId, action, changes);
        await _repository.AddAsync(log, ct);
    }
}
