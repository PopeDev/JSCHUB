using JSCHUB.Domain.Enums;

namespace JSCHUB.Application.DTOs;

/// <summary>
/// DTO para lectura de ReminderItem
/// </summary>
public record ReminderItemDto(
    Guid Id,
    string Title,
    string? Description,
    Category Category,
    List<string> Tags,
    string? Assignee,
    ItemStatus Status,
    ScheduleType ScheduleType,
    DateTime? DueAt,
    RecurrenceFrequency? RecurrenceFrequency,
    int? CustomIntervalDays,
    string Timezone,
    List<int> LeadTimeDays,
    DateTime? NextOccurrenceAt,
    DateTime? LastOccurrenceAt,
    Dictionary<string, string> Metadata,
    DateTime CreatedAt,
    DateTime UpdatedAt,
    int OpenAlertsCount
);

/// <summary>
/// DTO para creación de ReminderItem
/// </summary>
public record CreateReminderItemDto(
    string Title,
    string? Description,
    Category Category,
    List<string>? Tags,
    string? Assignee,
    ScheduleType ScheduleType,
    DateTime? DueAt,
    RecurrenceFrequency? RecurrenceFrequency,
    int? CustomIntervalDays,
    string? Timezone,
    List<int>? LeadTimeDays,
    Dictionary<string, string>? Metadata
);

/// <summary>
/// DTO para actualización de ReminderItem
/// </summary>
public record UpdateReminderItemDto(
    string Title,
    string? Description,
    Category Category,
    List<string>? Tags,
    string? Assignee,
    ItemStatus Status,
    ScheduleType ScheduleType,
    DateTime? DueAt,
    RecurrenceFrequency? RecurrenceFrequency,
    int? CustomIntervalDays,
    string? Timezone,
    List<int>? LeadTimeDays,
    Dictionary<string, string>? Metadata
);

/// <summary>
/// DTO para snooze
/// </summary>
public record SnoozeDto(DateTime SnoozedUntil);
