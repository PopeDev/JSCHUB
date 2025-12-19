using JSCHUB.Domain.Entities;

namespace JSCHUB.Domain.Interfaces;

/// <summary>
/// Repositorio para AuditLog
/// </summary>
public interface IAuditLogRepository
{
    Task<IEnumerable<AuditLog>> GetByEntityAsync(string entityType, Guid entityId, CancellationToken ct = default);
    Task AddAsync(AuditLog log, CancellationToken ct = default);
}
