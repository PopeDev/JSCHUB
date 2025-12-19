using JSCHUB.Domain.Enums;

namespace JSCHUB.Domain.Entities;

/// <summary>
/// Alerta generada automáticamente cuando un ReminderItem entra en ventana de aviso
/// </summary>
public class Alert
{
    public Guid Id { get; set; }
    
    /// <summary>ReminderItem que originó la alerta</summary>
    public Guid ReminderItemId { get; set; }
    public ReminderItem ReminderItem { get; set; } = null!;
    
    /// <summary>Estado de la alerta</summary>
    public AlertState State { get; set; } = AlertState.Open;
    
    /// <summary>Momento en que se disparó la alerta</summary>
    public DateTime TriggerAt { get; set; }
    
    /// <summary>Severidad basada en proximidad/vencimiento</summary>
    public AlertSeverity Severity { get; set; } = AlertSeverity.Info;
    
    /// <summary>Mensaje descriptivo generado</summary>
    public string Message { get; set; } = string.Empty;
    
    /// <summary>Fecha hasta la cual está pospuesta (si Snoozed)</summary>
    public DateTime? SnoozedUntil { get; set; }
    
    /// <summary>Ocurrencia específica a la que corresponde esta alerta</summary>
    public DateTime? OccurrenceAt { get; set; }
    
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    
    /// <summary>
    /// Calcula la severidad basándose en la proximidad del vencimiento
    /// </summary>
    public static AlertSeverity CalculateSeverity(DateTime occurrenceAt, DateTime now)
    {
        var daysUntil = (occurrenceAt - now).TotalDays;
        
        return daysUntil switch
        {
            < 0 => AlertSeverity.Critical,    // Vencido
            <= 3 => AlertSeverity.Critical,   // Muy próximo
            <= 7 => AlertSeverity.Warning,    // Próximo
            _ => AlertSeverity.Info           // Lejano
        };
    }
    
    /// <summary>
    /// Reconoce la alerta
    /// </summary>
    public void Acknowledge()
    {
        State = AlertState.Acknowledged;
        UpdatedAt = DateTime.UtcNow;
    }
    
    /// <summary>
    /// Pospone la alerta hasta una fecha específica
    /// </summary>
    public void Snooze(DateTime until)
    {
        State = AlertState.Snoozed;
        SnoozedUntil = until;
        UpdatedAt = DateTime.UtcNow;
    }
    
    /// <summary>
    /// Resuelve la alerta
    /// </summary>
    public void Resolve()
    {
        State = AlertState.Resolved;
        UpdatedAt = DateTime.UtcNow;
    }
}
