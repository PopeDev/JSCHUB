using JSCHUB.Domain.Entities;

namespace JSCHUB.Domain.Interfaces;

public interface IEventRepository
{
    /// <summary>
    /// Obtiene eventos en un rango de fechas UTC
    /// </summary>
    Task<IEnumerable<Event>> GetByRangeAsync(DateTime fromUtc, DateTime toUtc, CancellationToken ct = default);

    /// <summary>
    /// Obtiene un evento por su Id
    /// </summary>
    Task<Event?> GetByIdAsync(Guid id, CancellationToken ct = default);

    /// <summary>
    /// Añade un nuevo evento
    /// </summary>
    Task<Event> AddAsync(Event evento, CancellationToken ct = default);

    /// <summary>
    /// Actualiza un evento existente
    /// </summary>
    Task UpdateAsync(Event evento, CancellationToken ct = default);

    /// <summary>
    /// Elimina un evento
    /// </summary>
    Task DeleteAsync(Guid id, CancellationToken ct = default);

    /// <summary>
    /// Obtiene eventos pendientes de notificar dentro de una ventana de tiempo
    /// </summary>
    Task<IEnumerable<Event>> GetPendingNotificationAsync(DateTime fromUtc, DateTime toUtc, CancellationToken ct = default);

    /// <summary>
    /// Marca múltiples eventos como notificados
    /// </summary>
    Task MarkAsNotifiedAsync(IEnumerable<Guid> eventIds, CancellationToken ct = default);
}
