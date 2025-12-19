using JSCHUB.Domain.Enums;

namespace JSCHUB.Application.DTOs;

/// <summary>
/// DTO de lectura para Proyecto
/// </summary>
public record ProyectoDto(
    Guid Id,
    string Nombre,
    string? Descripcion,
    EstadoProyecto Estado,
    string? EnlacePrincipal,
    string? Etiquetas,
    string CreadoPor,
    DateTime CreadoEl,
    string ModificadoPor,
    DateTime ModificadoEl,
    int TotalEnlaces,
    int TotalRecursos
);

/// <summary>
/// DTO de lectura completo con enlaces y recursos
/// </summary>
public record ProyectoDetalleDto(
    Guid Id,
    string Nombre,
    string? Descripcion,
    EstadoProyecto Estado,
    string? EnlacePrincipal,
    string? Etiquetas,
    string CreadoPor,
    DateTime CreadoEl,
    string ModificadoPor,
    DateTime ModificadoEl,
    IEnumerable<EnlaceProyectoDto> Enlaces,
    IEnumerable<RecursoProyectoDto> Recursos
);

/// <summary>
/// DTO para crear un proyecto
/// </summary>
public record CreateProyectoDto(
    string Nombre,
    string? Descripcion,
    EstadoProyecto? Estado,
    string? EnlacePrincipal,
    string? Etiquetas
);

/// <summary>
/// DTO para actualizar un proyecto
/// </summary>
public record UpdateProyectoDto(
    string Nombre,
    string? Descripcion,
    EstadoProyecto Estado,
    string? EnlacePrincipal,
    string? Etiquetas
);
