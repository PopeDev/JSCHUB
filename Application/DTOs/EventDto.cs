namespace JSCHUB.Application.DTOs;

/// <summary>
/// DTO completo de un evento
/// </summary>
public record EventDto(
    Guid Id,
    string Title,
    DateTime StartUtc,
    DateTime EndUtc,
    string? MeetingUrl,
    DateTime? NotifiedAtUtc,
    string CreadoPor,
    DateTime CreadoEl,
    string ModificadoPor,
    DateTime ModificadoEl
);

/// <summary>
/// DTO para crear un nuevo evento
/// </summary>
public record CreateEventDto(
    string Title,
    DateTime StartUtc,
    DateTime EndUtc,
    string? MeetingUrl
);

/// <summary>
/// DTO para actualizar un evento existente
/// </summary>
public record UpdateEventDto(
    string Title,
    DateTime StartUtc,
    DateTime EndUtc,
    string? MeetingUrl
);

/// <summary>
/// DTO para representar un día del calendario con sus eventos
/// </summary>
public record CalendarDayDto(
    DateTime Date,
    bool IsCurrentMonth,
    bool IsToday,
    List<CalendarItemDto> Events
);

/// <summary>
/// DTO para representar un evento en el calendario (vista resumida)
/// </summary>
public record CalendarItemDto(
    Guid Id,
    string Title,
    DateTime StartUtc,
    DateTime EndUtc,
    string? MeetingUrl,
    bool HasMeetingUrl
)
{
    /// <summary>
    /// Hora de inicio formateada en hora local
    /// </summary>
    public string StartTimeFormatted => StartUtc.ToLocalTime().ToString("HH:mm");
}

/// <summary>
/// DTO para representar un mes completo del calendario
/// </summary>
public record CalendarMonthDto(
    int Year,
    int Month,
    string MonthName,
    List<CalendarDayDto> Days
);
