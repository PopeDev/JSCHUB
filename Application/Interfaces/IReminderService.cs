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
        Guid usuarioActualId,
        string? searchText = null,
        Category? category = null,
        ItemStatus? status = null,
        string? tag = null,
        Guid? asignadoAId = null,
        Guid? proyectoId = null,
        int skip = 0,
        int take = 50,
        CancellationToken ct = default);

    Task<int> CountAsync(
        Guid usuarioActualId,
        string? searchText = null,
        Category? category = null,
        ItemStatus? status = null,
        string? tag = null,
        Guid? asignadoAId = null,
        Guid? proyectoId = null,
        CancellationToken ct = default);

    /// <summary>
    /// Crea un recordatorio. El usuario debe tener acceso a los proyectos especificados.
    /// Si se asigna a un usuario, ese usuario debe tener acceso a los proyectos del recordatorio.
    /// </summary>
    Task<ReminderItemDto> CreateAsync(Guid usuarioActualId, CreateReminderItemDto dto, CancellationToken ct = default);

    Task<ReminderItemDto> UpdateAsync(Guid usuarioActualId, Guid id, UpdateReminderItemDto dto, CancellationToken ct = default);
    Task DeleteAsync(Guid usuarioActualId, Guid id, CancellationToken ct = default);
    Task<ReminderItemDto> CompleteAsync(Guid usuarioActualId, Guid id, CancellationToken ct = default);
    Task<ReminderItemDto> SnoozeAsync(Guid usuarioActualId, Guid id, DateTime snoozedUntil, CancellationToken ct = default);
    Task<ReminderItemDto> PauseAsync(Guid usuarioActualId, Guid id, CancellationToken ct = default);
    Task<ReminderItemDto> ResumeAsync(Guid usuarioActualId, Guid id, CancellationToken ct = default);
}
