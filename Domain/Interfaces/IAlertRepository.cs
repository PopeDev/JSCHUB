using JSCHUB.Domain.Entities;
using JSCHUB.Domain.Enums;

namespace JSCHUB.Domain.Interfaces;

/// <summary>
/// Repositorio para Alert
/// </summary>
public interface IAlertRepository
{
    Task<Alert?> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<IEnumerable<Alert>> GetByReminderItemIdAsync(Guid reminderItemId, CancellationToken ct = default);
    Task<IEnumerable<Alert>> SearchAsync(
        AlertState? state = null,
        AlertSeverity? severity = null,
        DateTime? fromDate = null,
        DateTime? toDate = null,
        int skip = 0,
        int take = 50,
        CancellationToken ct = default);
    Task<int> CountByStateAsync(AlertState state, CancellationToken ct = default);
    Task<int> CountOverdueAsync(CancellationToken ct = default);
    Task<int> CountDueTodayAsync(CancellationToken ct = default);
    Task<int> CountDueInDaysAsync(int days, CancellationToken ct = default);
    Task<Alert?> FindExistingAsync(Guid reminderItemId, DateTime occurrenceAt, CancellationToken ct = default);
    Task AddAsync(Alert alert, CancellationToken ct = default);
    Task UpdateAsync(Alert alert, CancellationToken ct = default);
}
