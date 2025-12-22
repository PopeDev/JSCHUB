using JSCHUB.Domain.Entities;
using JSCHUB.Domain.Enums;

namespace JSCHUB.Domain.Interfaces;

/// <summary>
/// Repositorio para gestión de asignaciones Usuario-Proyecto
/// </summary>
public interface IUsuarioProyectoRepository
{
    /// <summary>
    /// Obtiene la asignación de un usuario a un proyecto
    /// </summary>
    Task<UsuarioProyecto?> GetAsync(Guid usuarioId, Guid proyectoId, CancellationToken ct = default);

    /// <summary>
    /// Obtiene todos los proyectos asignados a un usuario
    /// </summary>
    Task<IEnumerable<UsuarioProyecto>> GetProyectosByUsuarioAsync(Guid usuarioId, CancellationToken ct = default);

    /// <summary>
    /// Obtiene todos los usuarios asignados a un proyecto
    /// </summary>
    Task<IEnumerable<UsuarioProyecto>> GetUsuariosByProyectoAsync(Guid proyectoId, CancellationToken ct = default);

    /// <summary>
    /// Verifica si un usuario tiene acceso a un proyecto
    /// </summary>
    Task<bool> TieneAccesoAsync(Guid usuarioId, Guid proyectoId, CancellationToken ct = default);

    /// <summary>
    /// Obtiene el rol de un usuario en un proyecto (null si no tiene acceso)
    /// </summary>
    Task<RolProyecto?> GetRolAsync(Guid usuarioId, Guid proyectoId, CancellationToken ct = default);

    /// <summary>
    /// Verifica si un usuario tiene acceso a todos los proyectos especificados
    /// </summary>
    Task<bool> TieneAccesoATodosAsync(Guid usuarioId, IEnumerable<Guid> proyectoIds, CancellationToken ct = default);

    /// <summary>
    /// Obtiene el Proyecto General
    /// </summary>
    Task<Proyecto?> GetProyectoGeneralAsync(CancellationToken ct = default);

    /// <summary>
    /// Asigna un usuario a un proyecto
    /// </summary>
    Task<UsuarioProyecto> AddAsync(UsuarioProyecto asignacion, CancellationToken ct = default);

    /// <summary>
    /// Actualiza una asignación (cambio de rol)
    /// </summary>
    Task UpdateAsync(UsuarioProyecto asignacion, CancellationToken ct = default);

    /// <summary>
    /// Elimina la asignación de un usuario a un proyecto
    /// </summary>
    Task DeleteAsync(Guid usuarioId, Guid proyectoId, CancellationToken ct = default);

    /// <summary>
    /// Cuenta los gastos no saldados de un usuario en un proyecto
    /// </summary>
    Task<int> CountGastosNoSaldadosAsync(Guid usuarioId, Guid proyectoId, CancellationToken ct = default);
}
