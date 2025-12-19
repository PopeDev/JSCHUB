namespace JSCHUB.Domain.Enums;

/// <summary>
/// Estado de una alerta
/// </summary>
public enum AlertState
{
    /// <summary>Abierta, pendiente de acción</summary>
    Open,
    
    /// <summary>Reconocida pero no resuelta</summary>
    Acknowledged,
    
    /// <summary>Pospuesta hasta cierta fecha</summary>
    Snoozed,
    
    /// <summary>Resuelta/completada</summary>
    Resolved
}
