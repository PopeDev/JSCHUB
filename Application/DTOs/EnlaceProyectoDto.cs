using JSCHUB.Domain.Enums;

namespace JSCHUB.Application.DTOs;

/// <summary>
/// DTO de lectura para EnlaceProyecto
/// </summary>
public record EnlaceProyectoDto(
    Guid Id,
    Guid ProyectoId,
    string Titulo,
    string Url,
    string? Descripcion,
    TipoEnlace Tipo,
    int Orden,
    string CreadoPor,
    DateTime CreadoEl,
    string ModificadoPor,
    DateTime ModificadoEl
);

/// <summary>
/// DTO para crear un enlace de proyecto
/// </summary>
public record CreateEnlaceProyectoDto(
    Guid ProyectoId,
    string Titulo,
    string Url,
    string? Descripcion,
    TipoEnlace? Tipo,
    int? Orden
);

/// <summary>
/// DTO para actualizar un enlace de proyecto
/// </summary>
public record UpdateEnlaceProyectoDto(
    string Titulo,
    string Url,
    string? Descripcion,
    TipoEnlace Tipo,
    int Orden
);
