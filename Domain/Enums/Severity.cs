namespace JSCHUB.Domain.Enums;

/// <summary>
/// Severidad de una alerta
/// </summary>
public enum AlertSeverity
{
    /// <summary>Informativa (lejana en el tiempo)</summary>
    Info,
    
    /// <summary>Advertencia (próxima)</summary>
    Warning,
    
    /// <summary>Crítica (vencida o muy próxima)</summary>
    Critical
}
