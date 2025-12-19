namespace JSCHUB.Domain.Enums;

/// <summary>
/// Categoría del recordatorio/renovación
/// </summary>
public enum Category
{
    /// <summary>Renovación (dominios, hosting, licencias, etc.)</summary>
    Renewal,
    
    /// <summary>Recordatorio genérico (impuestos, citas, etc.)</summary>
    Reminder,
    
    /// <summary>Otro tipo</summary>
    Other
}
