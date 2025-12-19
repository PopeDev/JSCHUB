using JSCHUB.Domain.Enums;

namespace JSCHUB.Domain.Entities;

/// <summary>
/// Entidad principal para gestionar renovaciones y recordatorios de forma genérica.
/// Todo tipo de evento (dominio, hosting, impuestos, etc.) se modela con esta entidad
/// usando el campo Metadata para datos específicos del caso.
/// </summary>
public class ReminderItem
{
    public Guid Id { get; set; }
    
    /// <summary>Título descriptivo</summary>
    public string Title { get; set; } = string.Empty;
    
    /// <summary>Descripción opcional</summary>
    public string? Description { get; set; }
    
    /// <summary>Categoría: Renewal, Reminder, Other</summary>
    public Category Category { get; set; } = Category.Reminder;
    
    /// <summary>Etiquetas para filtrado</summary>
    public List<string> Tags { get; set; } = [];
    
    /// <summary>Responsable asignado</summary>
    public string? Assignee { get; set; }
    
    /// <summary>Estado del item: Active, Paused, Archived</summary>
    public ItemStatus Status { get; set; } = ItemStatus.Active;
    
    /// <summary>Tipo de programación: OneTime o Recurring</summary>
    public ScheduleType ScheduleType { get; set; } = ScheduleType.OneTime;
    
    /// <summary>Fecha de vencimiento (requerido si OneTime)</summary>
    public DateTime? DueAt { get; set; }
    
    /// <summary>Frecuencia de recurrencia (requerido si Recurring)</summary>
    public RecurrenceFrequency? RecurrenceFrequency { get; set; }
    
    /// <summary>Intervalo de recurrencia personalizado en días (para Custom)</summary>
    public int? CustomIntervalDays { get; set; }
    
    /// <summary>Zona horaria</summary>
    public string Timezone { get; set; } = "Europe/Madrid";
    
    /// <summary>Días de antelación para generar alertas</summary>
    public List<int> LeadTimeDays { get; set; } = [30];
    
    /// <summary>Próxima ocurrencia calculada</summary>
    public DateTime? NextOccurrenceAt { get; set; }
    
    /// <summary>Última ocurrencia completada</summary>
    public DateTime? LastOccurrenceAt { get; set; }
    
    /// <summary>Metadatos adicionales (domainName, provider, invoiceRef, etc.)</summary>
    public Dictionary<string, string> Metadata { get; set; } = new();
    
    // Auditoría
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    public string CreatedBy { get; set; } = "admin";
    public string UpdatedBy { get; set; } = "admin";
    
    // Navegación
    public ICollection<Alert> Alerts { get; set; } = [];
    
    /// <summary>
    /// Calcula la próxima ocurrencia basándose en la frecuencia de recurrencia
    /// </summary>
    public DateTime? CalculateNextOccurrence(DateTime from)
    {
        if (ScheduleType == ScheduleType.OneTime)
        {
            return DueAt > from ? DueAt : null;
        }
        
        if (RecurrenceFrequency == null) return null;
        
        var baseDate = LastOccurrenceAt ?? DueAt ?? from;
        
        return RecurrenceFrequency switch
        {
            Enums.RecurrenceFrequency.Weekly => baseDate.AddDays(7),
            Enums.RecurrenceFrequency.Monthly => baseDate.AddMonths(1),
            Enums.RecurrenceFrequency.Quarterly => baseDate.AddMonths(3),
            Enums.RecurrenceFrequency.Yearly => baseDate.AddYears(1),
            Enums.RecurrenceFrequency.Custom => CustomIntervalDays.HasValue 
                ? baseDate.AddDays(CustomIntervalDays.Value) 
                : null,
            _ => null
        };
    }
    
    /// <summary>
    /// Marca la ocurrencia actual como completada y programa la siguiente
    /// </summary>
    public void Complete()
    {
        LastOccurrenceAt = NextOccurrenceAt ?? DateTime.UtcNow;
        
        if (ScheduleType == ScheduleType.Recurring)
        {
            NextOccurrenceAt = CalculateNextOccurrence(DateTime.UtcNow);
        }
        else
        {
            NextOccurrenceAt = null;
            Status = ItemStatus.Archived;
        }
        
        UpdatedAt = DateTime.UtcNow;
    }
}
