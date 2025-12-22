using JSCHUB.Domain.Enums;

namespace JSCHUB.Application.DTOs;

/// <summary>
/// DTO de lectura para asignación Usuario-Proyecto
/// </summary>
public record UsuarioProyectoDto(
    Guid UsuarioId,
    string UsuarioNombre,
    Guid ProyectoId,
    string ProyectoNombre,
    bool ProyectoEsGeneral,
    RolProyecto Rol,
    DateTime FechaAsignacion,
    string AsignadoPor
);

/// <summary>
/// DTO simplificado de proyecto asignado a un usuario
/// </summary>
public record ProyectoAsignadoDto(
    Guid Id,
    string Nombre,
    bool EsGeneral,
    RolProyecto Rol
);

/// <summary>
/// DTO simplificado de usuario asignado a un proyecto
/// </summary>
public record UsuarioAsignadoDto(
    Guid Id,
    string Nombre,
    RolProyecto Rol,
    DateTime FechaAsignacion
);

/// <summary>
/// DTO para asignar un usuario a un proyecto
/// </summary>
public record AsignarUsuarioProyectoDto(
    Guid UsuarioId,
    Guid ProyectoId,
    RolProyecto Rol
);

/// <summary>
/// DTO para actualizar el rol de un usuario en un proyecto
/// </summary>
public record ActualizarRolUsuarioProyectoDto(
    Guid UsuarioId,
    Guid ProyectoId,
    RolProyecto NuevoRol
);
