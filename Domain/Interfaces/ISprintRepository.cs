using JSCHUB.Domain.Entities;
using JSCHUB.Domain.Enums;

namespace JSCHUB.Domain.Interfaces;

/// <summary>
/// Repositorio para gestión de Sprints
/// </summary>
public interface ISprintRepository
{
    /// <summary>
    /// Obtiene un sprint por ID
    /// </summary>
    Task<Sprint?> GetByIdAsync(Guid id, CancellationToken ct = default);

    /// <summary>
    /// Obtiene un sprint por ID con sus tareas y proyecto
    /// </summary>
    Task<Sprint?> GetByIdWithDetailsAsync(Guid id, CancellationToken ct = default);

    /// <summary>
    /// Obtiene todos los sprints de un proyecto
    /// </summary>
    Task<IEnumerable<Sprint>> GetByProyectoIdAsync(Guid proyectoId, CancellationToken ct = default);

    /// <summary>
    /// Obtiene el sprint activo de un proyecto
    /// </summary>
    Task<Sprint?> GetSprintActivoAsync(Guid proyectoId, CancellationToken ct = default);

    /// <summary>
    /// Obtiene sprints por estado
    /// </summary>
    Task<IEnumerable<Sprint>> GetByEstadoAsync(Guid proyectoId, EstadoSprint estado, CancellationToken ct = default);

    /// <summary>
    /// Crea un nuevo sprint
    /// </summary>
    Task<Sprint> AddAsync(Sprint sprint, CancellationToken ct = default);

    /// <summary>
    /// Actualiza un sprint existente
    /// </summary>
    Task UpdateAsync(Sprint sprint, CancellationToken ct = default);

    /// <summary>
    /// Elimina un sprint (solo si está Pendiente y no tiene tareas)
    /// </summary>
    Task DeleteAsync(Sprint sprint, CancellationToken ct = default);

    /// <summary>
    /// Obtiene el histórico de tareas de un sprint cerrado
    /// </summary>
    Task<IEnumerable<SprintTareaHistorico>> GetHistoricoAsync(Guid sprintId, CancellationToken ct = default);

    /// <summary>
    /// Añade un registro histórico de tarea
    /// </summary>
    Task AddHistoricoAsync(SprintTareaHistorico historico, CancellationToken ct = default);

    /// <summary>
    /// Añade múltiples registros históricos de tareas
    /// </summary>
    Task AddHistoricoRangeAsync(IEnumerable<SprintTareaHistorico> historicos, CancellationToken ct = default);
}
