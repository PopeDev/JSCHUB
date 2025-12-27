namespace JSCHUB.Domain.Entities;

/// <summary>
/// Representa un evento del calendario
/// </summary>
public class Event
{
    public Guid Id { get; set; }

    /// <summary>
    /// Título del evento
    /// </summary>
    public string Title { get; set; } = string.Empty;

    /// <summary>
    /// Fecha/hora de inicio en UTC
    /// </summary>
    public DateTime StartUtc { get; set; }

    /// <summary>
    /// Fecha/hora de fin en UTC
    /// </summary>
    public DateTime EndUtc { get; set; }

    /// <summary>
    /// URL de reunión (Teams, Meet, Zoom, etc.) - opcional
    /// </summary>
    public string? MeetingUrl { get; set; }

    /// <summary>
    /// Fecha/hora en que se envió la notificación (null si no se ha notificado)
    /// </summary>
    public DateTime? NotifiedAtUtc { get; set; }

    // Auditoría
    public string CreadoPor { get; set; } = "sistema";
    public DateTime CreadoEl { get; set; } = DateTime.UtcNow;
    public string ModificadoPor { get; set; } = "sistema";
    public DateTime ModificadoEl { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Indica si el evento ya fue notificado
    /// </summary>
    public bool YaNotificado => NotifiedAtUtc.HasValue;

    /// <summary>
    /// Marca el evento como notificado
    /// </summary>
    public void MarcarNotificado()
    {
        NotifiedAtUtc = DateTime.UtcNow;
    }
}
