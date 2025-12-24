using JSCHUB.Application.DTOs;
using JSCHUB.Domain.Enums;

namespace JSCHUB.Application.Interfaces;

/// <summary>
/// Servicio para gestión de Sprints
/// </summary>
public interface ISprintService
{
    /// <summary>
    /// Obtiene un sprint por ID
    /// </summary>
    Task<SprintDto?> GetByIdAsync(Guid id, CancellationToken ct = default);

    /// <summary>
    /// Obtiene todos los sprints de un proyecto
    /// </summary>
    Task<IEnumerable<SprintDto>> GetByProyectoIdAsync(Guid proyectoId, CancellationToken ct = default);

    /// <summary>
    /// Obtiene el sprint activo de un proyecto
    /// </summary>
    Task<SprintDto?> GetSprintActivoAsync(Guid proyectoId, CancellationToken ct = default);

    /// <summary>
    /// Crea un nuevo sprint
    /// </summary>
    Task<SprintDto> CreateAsync(CreateSprintDto dto, string usuario, CancellationToken ct = default);

    /// <summary>
    /// Actualiza un sprint existente
    /// </summary>
    Task<SprintDto> UpdateAsync(Guid id, UpdateSprintDto dto, string usuario, CancellationToken ct = default);

    /// <summary>
    /// Activa un sprint (convierte el sprint en Activo y lo asigna al proyecto)
    /// </summary>
    Task ActivarSprintAsync(Guid id, string usuario, CancellationToken ct = default);

    /// <summary>
    /// Cierra el sprint activo y opcionalmente activa el siguiente
    /// </summary>
    Task<CierreSprintResultDto> CerrarSprintActivoAsync(Guid proyectoId, Guid? siguienteSprintId, string usuario, CancellationToken ct = default);

    /// <summary>
    /// Elimina un sprint (solo si está pendiente)
    /// </summary>
    Task DeleteAsync(Guid id, CancellationToken ct = default);
}
