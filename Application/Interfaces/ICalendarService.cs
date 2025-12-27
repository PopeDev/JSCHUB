using JSCHUB.Application.DTOs;

namespace JSCHUB.Application.Interfaces;

public interface ICalendarService
{
    /// <summary>
    /// Obtiene los datos del calendario para un mes específico
    /// </summary>
    Task<CalendarMonthDto> GetMonthAsync(int year, int month, CancellationToken ct = default);

    /// <summary>
    /// Obtiene un evento por su Id
    /// </summary>
    Task<EventDto?> GetByIdAsync(Guid id, CancellationToken ct = default);

    /// <summary>
    /// Crea un nuevo evento
    /// </summary>
    Task<EventDto> CreateAsync(CreateEventDto dto, string usuario, CancellationToken ct = default);

    /// <summary>
    /// Actualiza un evento existente
    /// </summary>
    Task<EventDto> UpdateAsync(Guid id, UpdateEventDto dto, string usuario, CancellationToken ct = default);

    /// <summary>
    /// Elimina un evento
    /// </summary>
    Task DeleteAsync(Guid id, CancellationToken ct = default);
}
