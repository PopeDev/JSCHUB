using JSCHUB.Domain.Entities;

namespace JSCHUB.Application.Interfaces;

/// <summary>
/// Servicio de auditoría
/// </summary>
public interface IAuditService
{
    Task<IEnumerable<AuditLog>> GetByEntityAsync(string entityType, Guid entityId, CancellationToken ct = default);
    Task LogAsync(string entityType, Guid entityId, string action, string? changes = null, CancellationToken ct = default);
}
