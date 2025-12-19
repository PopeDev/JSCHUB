using JSCHUB.Application.DTOs;
using JSCHUB.Domain.Enums;

namespace JSCHUB.Application.Interfaces;

/// <summary>
/// Servicio para gestionar ReminderItems
/// </summary>
public interface IReminderService
{
    Task<ReminderItemDto?> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<IEnumerable<ReminderItemDto>> SearchAsync(
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
    Task<ReminderItemDto> CreateAsync(CreateReminderItemDto dto, CancellationToken ct = default);
    Task<ReminderItemDto> UpdateAsync(Guid id, UpdateReminderItemDto dto, CancellationToken ct = default);
    Task DeleteAsync(Guid id, CancellationToken ct = default);
    Task<ReminderItemDto> CompleteAsync(Guid id, CancellationToken ct = default);
    Task<ReminderItemDto> SnoozeAsync(Guid id, DateTime snoozedUntil, CancellationToken ct = default);
    Task<ReminderItemDto> PauseAsync(Guid id, CancellationToken ct = default);
    Task<ReminderItemDto> ResumeAsync(Guid id, CancellationToken ct = default);
}
