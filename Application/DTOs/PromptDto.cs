namespace JSCHUB.Application.DTOs;

/// <summary>
/// DTO para lectura de Prompt (listado)
/// </summary>
public record PromptDto(
    Guid Id,
    string Title,
    string Description,
    bool Activo,
    Guid? ProyectoId,
    string? ProyectoNombre,
    Guid CreatedByUserId,
    string CreatedByUserNombre,
    Guid ToolId,
    string ToolName,
    IEnumerable<TagDto> Tags,
    DateTime CreadoEl,
    DateTime ModificadoEl
);

/// <summary>
/// DTO para creación de Prompt
/// </summary>
public record CreatePromptDto(
    string Title,
    string Description,
    Guid? ProyectoId,
    Guid CreatedByUserId,
    Guid ToolId,
    IEnumerable<Guid> TagIds
);

/// <summary>
/// DTO para actualización de Prompt
/// </summary>
public record UpdatePromptDto(
    string Title,
    string Description,
    Guid? ProyectoId,
    Guid ToolId,
    IEnumerable<Guid> TagIds,
    bool Activo
);
