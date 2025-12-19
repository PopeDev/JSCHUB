using JSCHUB.Application.DTOs;
using JSCHUB.Domain.Enums;

namespace JSCHUB.Application.Interfaces;

/// <summary>
/// Servicio para gestionar Alerts
/// </summary>
public interface IAlertService
{
    Task<AlertDto?> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<IEnumerable<AlertDto>> SearchAsync(
        AlertState? state = null,
        AlertSeverity? severity = null,
        DateTime? fromDate = null,
        DateTime? toDate = null,
        int skip = 0,
        int take = 50,
        CancellationToken ct = default);
    Task<IEnumerable<AlertDto>> GetByReminderItemIdAsync(Guid reminderItemId, CancellationToken ct = default);
    Task<AlertStatsDto> GetStatsAsync(CancellationToken ct = default);
    Task AcknowledgeAsync(Guid id, CancellationToken ct = default);
    Task SnoozeAsync(Guid id, DateTime snoozedUntil, CancellationToken ct = default);
    Task ResolveAsync(Guid id, CancellationToken ct = default);
    Task GenerateAlertsAsync(CancellationToken ct = default);
}
