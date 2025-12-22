using JSCHUB.Application.DTOs;
using JSCHUB.Domain.Enums;

namespace JSCHUB.Application.Interfaces;

/// <summary>
/// Servicio para gestión de asignaciones Usuario-Proyecto
/// </summary>
public interface IUsuarioProyectoService
{
    /// <summary>
    /// Obtiene los proyectos asignados a un usuario (activos, no archivados)
    /// </summary>
    Task<IEnumerable<ProyectoAsignadoDto>> GetProyectosDelUsuarioAsync(Guid usuarioId, CancellationToken ct = default);

    /// <summary>
    /// Obtiene los usuarios asignados a un proyecto
    /// </summary>
    Task<IEnumerable<UsuarioAsignadoDto>> GetUsuariosDelProyectoAsync(Guid proyectoId, CancellationToken ct = default);

    /// <summary>
    /// Verifica si un usuario tiene acceso a un proyecto
    /// </summary>
    Task<bool> TieneAccesoAsync(Guid usuarioId, Guid proyectoId, CancellationToken ct = default);

    /// <summary>
    /// Obtiene el rol de un usuario en un proyecto
    /// </summary>
    Task<RolProyecto?> GetRolAsync(Guid usuarioId, Guid proyectoId, CancellationToken ct = default);

    /// <summary>
    /// Verifica si un usuario tiene acceso a todos los proyectos especificados
    /// </summary>
    Task<bool> TieneAccesoATodosAsync(Guid usuarioId, IEnumerable<Guid> proyectoIds, CancellationToken ct = default);

    /// <summary>
    /// Verifica si un usuario puede realizar una acción según su rol
    /// </summary>
    Task<bool> PuedeRealizarAccionAsync(Guid usuarioId, Guid proyectoId, AccionProyecto accion, CancellationToken ct = default);

    /// <summary>
    /// Obtiene el Proyecto General
    /// </summary>
    Task<ProyectoSimpleDto?> GetProyectoGeneralAsync(CancellationToken ct = default);

    /// <summary>
    /// Asigna un usuario a un proyecto
    /// </summary>
    Task<UsuarioProyectoDto> AsignarUsuarioAsync(AsignarUsuarioProyectoDto dto, string asignadoPor, CancellationToken ct = default);

    /// <summary>
    /// Actualiza el rol de un usuario en un proyecto
    /// </summary>
    Task<UsuarioProyectoDto> ActualizarRolAsync(ActualizarRolUsuarioProyectoDto dto, CancellationToken ct = default);

    /// <summary>
    /// Desasigna un usuario de un proyecto (solo si no tiene gastos pendientes)
    /// </summary>
    Task DesasignarUsuarioAsync(Guid usuarioId, Guid proyectoId, CancellationToken ct = default);
}

/// <summary>
/// Acciones que se pueden realizar en un proyecto
/// </summary>
public enum AccionProyecto
{
    Ver,
    CrearItem,
    EditarItemPropio,
    EditarItemOtros,
    EliminarItemPropio,
    EliminarItemOtros,
    GestionarProyecto,
    GestionarUsuarios,
    GestionarKanban
}
