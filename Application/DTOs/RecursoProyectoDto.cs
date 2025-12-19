using JSCHUB.Domain.Enums;

namespace JSCHUB.Application.DTOs;

/// <summary>
/// DTO de lectura para RecursoProyecto
/// </summary>
public record RecursoProyectoDto(
    Guid Id,
    Guid ProyectoId,
    string Nombre,
    TipoRecurso Tipo,
    string? Url,
    string? Contenido,
    string? Etiquetas,
    string CreadoPor,
    DateTime CreadoEl,
    string ModificadoPor,
    DateTime ModificadoEl
);

/// <summary>
/// DTO para crear un recurso de proyecto
/// </summary>
public record CreateRecursoProyectoDto(
    Guid ProyectoId,
    string Nombre,
    TipoRecurso Tipo,
    string? Url,
    string? Contenido,
    string? Etiquetas
);

/// <summary>
/// DTO para actualizar un recurso de proyecto
/// </summary>
public record UpdateRecursoProyectoDto(
    string Nombre,
    TipoRecurso Tipo,
    string? Url,
    string? Contenido,
    string? Etiquetas
);
