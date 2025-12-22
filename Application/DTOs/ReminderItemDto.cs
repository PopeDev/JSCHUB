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
    Guid? AsignadoAId,
    string? AsignadoANombre,
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
    int OpenAlertsCount,
    List<ProyectoSimpleDto> Proyectos,
    bool EsGeneral // true si solo está asociado al Proyecto General
);

/// <summary>
/// DTO para creación de ReminderItem
/// </summary>
public record CreateReminderItemDto(
    string Title,
    string? Description,
    Category Category,
    List<string>? Tags,
    Guid? AsignadoAId, // FK al Usuario (debe tener acceso a los proyectos)
    ScheduleType ScheduleType,
    DateTime? DueAt,
    RecurrenceFrequency? RecurrenceFrequency,
    int? CustomIntervalDays,
    string? Timezone,
    List<int>? LeadTimeDays,
    Dictionary<string, string>? Metadata,
    List<Guid>? ProyectoIds // Si es null o vacío, se asocia al Proyecto General
);

/// <summary>
/// DTO para actualización de ReminderItem
/// </summary>
public record UpdateReminderItemDto(
    string Title,
    string? Description,
    Category Category,
    List<string>? Tags,
    Guid? AsignadoAId, // FK al Usuario (debe tener acceso a los proyectos)
    ItemStatus Status,
    ScheduleType ScheduleType,
    DateTime? DueAt,
    RecurrenceFrequency? RecurrenceFrequency,
    int? CustomIntervalDays,
    string? Timezone,
    List<int>? LeadTimeDays,
    Dictionary<string, string>? Metadata,
    List<Guid>? ProyectoIds // Si es null o vacío, se asocia al Proyecto General
);

/// <summary>
/// DTO para snooze
/// </summary>
public record SnoozeDto(DateTime SnoozedUntil);
