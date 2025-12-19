namespace JSCHUB.Domain.Enums;

/// <summary>
/// Estado del ReminderItem
/// </summary>
public enum ItemStatus
{
    /// <summary>Activo y generando alertas</summary>
    Active,
    
    /// <summary>Pausado temporalmente</summary>
    Paused,
    
    /// <summary>Archivado (soft delete)</summary>
    Archived
}
