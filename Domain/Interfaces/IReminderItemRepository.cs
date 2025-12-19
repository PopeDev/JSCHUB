using JSCHUB.Domain.Entities;
using JSCHUB.Domain.Enums;

namespace JSCHUB.Domain.Interfaces;

/// <summary>
/// Repositorio para ReminderItem
/// </summary>
public interface IReminderItemRepository
{
    Task<ReminderItem?> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<ReminderItem?> GetByIdWithAlertsAsync(Guid id, CancellationToken ct = default);
    Task<IEnumerable<ReminderItem>> GetAllAsync(CancellationToken ct = default);
    Task<IEnumerable<ReminderItem>> GetActiveAsync(CancellationToken ct = default);
    Task<IEnumerable<ReminderItem>> SearchAsync(
        string? searchText = null,
        Category? category = null,
        ItemStatus? status = null,
        string? tag = null,
        string? assignee = null,
        int skip = 0,
        int take = 50,
        CancellationToken ct = default);
    Task<int> CountAsync(
        string? searchText = null,
        Category? category = null,
        ItemStatus? status = null,
        string? tag = null,
        string? assignee = null,
        CancellationToken ct = default);
    Task AddAsync(ReminderItem item, CancellationToken ct = default);
    Task UpdateAsync(ReminderItem item, CancellationToken ct = default);
    Task DeleteAsync(Guid id, CancellationToken ct = default);
}
