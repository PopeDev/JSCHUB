namespace JSCHUB.Application.DTOs;

/// <summary>
/// DTO para lectura de Persona
/// </summary>
public record PersonaDto(
    Guid Id,
    string Nombre,
    string? Email,
    string? Telefono,
    bool Activo
);

/// <summary>
/// DTO para creación de Persona
/// </summary>
public record CreatePersonaDto(
    string Nombre,
    string? Email,
    string? Telefono
);

/// <summary>
/// DTO para actualización de Persona
/// </summary>
public record UpdatePersonaDto(
    string Nombre,
    string? Email,
    string? Telefono,
    bool Activo
);
