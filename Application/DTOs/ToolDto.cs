namespace JSCHUB.Application.DTOs;

/// <summary>
/// DTO para lectura de Tool
/// </summary>
public record ToolDto(
    Guid Id,
    string Name,
    bool Activo,
    DateTime CreadoEl
);

/// <summary>
/// DTO para creación de Tool
/// </summary>
public record CreateToolDto(
    string Name
);

/// <summary>
/// DTO para actualización de Tool
/// </summary>
public record UpdateToolDto(
    string Name,
    bool Activo
);
