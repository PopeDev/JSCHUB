namespace JSCHUB.Application.DTOs;

/// <summary>
/// DTO para lectura de Tag
/// </summary>
public record TagDto(
    Guid Id,
    string Name,
    bool Activo,
    DateTime CreadoEl
);

/// <summary>
/// DTO para creación de Tag
/// </summary>
public record CreateTagDto(
    string Name
);

/// <summary>
/// DTO para actualización de Tag
/// </summary>
public record UpdateTagDto(
    string Name,
    bool Activo
);
