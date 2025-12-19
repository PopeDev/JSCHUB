namespace JSCHUB.Domain.Entities;

/// <summary>
/// Registro de auditoría para tracking de cambios
/// </summary>
public class AuditLog
{
    public Guid Id { get; set; }
    
    /// <summary>Tipo de entidad afectada</summary>
    public string EntityType { get; set; } = string.Empty;
    
    /// <summary>ID de la entidad afectada</summary>
    public Guid EntityId { get; set; }
    
    /// <summary>Acción realizada: Create, Update, Delete, Complete, Snooze, etc.</summary>
    public string Action { get; set; } = string.Empty;
    
    /// <summary>Cambios en formato JSON</summary>
    public string? Changes { get; set; }
    
    /// <summary>Usuario que realizó la acción</summary>
    public string User { get; set; } = "admin";
    
    /// <summary>Momento de la acción</summary>
    public DateTime At { get; set; } = DateTime.UtcNow;
    
    public static AuditLog Create(string entityType, Guid entityId, string action, string? changes = null, string user = "admin")
    {
        return new AuditLog
        {
            Id = Guid.NewGuid(),
            EntityType = entityType,
            EntityId = entityId,
            Action = action,
            Changes = changes,
            User = user,
            At = DateTime.UtcNow
        };
    }
}
