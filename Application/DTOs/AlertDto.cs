using JSCHUB.Domain.Enums;

namespace JSCHUB.Application.DTOs;

/// <summary>
/// DTO para lectura de Alert
/// </summary>
public record AlertDto(
    Guid Id,
    Guid ReminderItemId,
    string ReminderItemTitle,
    Category ReminderItemCategory,
    AlertState State,
    DateTime TriggerAt,
    AlertSeverity Severity,
    string Message,
    DateTime? SnoozedUntil,
    DateTime? OccurrenceAt,
    DateTime CreatedAt
);

/// <summary>
/// Estadísticas del dashboard
/// </summary>
public record AlertStatsDto(
    int TotalOpen,
    int DueToday,
    int DueNext7Days,
    int Overdue,
    int Snoozed,
    int ResolvedToday
);
