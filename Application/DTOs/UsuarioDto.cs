namespace JSCHUB.Application.DTOs;

/// <summary>
/// DTO para lectura de Usuario
/// </summary>
public record UsuarioDto(
    Guid Id,
    string Nombre,
    string? Email,
    string? Telefono,
    bool Activo,
    IEnumerable<ProyectoAsignadoDto> Proyectos
);

/// <summary>
/// DTO para creación de Usuario
/// </summary>
public record CreateUsuarioDto(
    string Nombre,
    string? Email,
    string? Telefono
);

/// <summary>
/// DTO para actualización de Usuario
/// </summary>
public record UpdateUsuarioDto(
    string Nombre,
    string? Email,
    string? Telefono,
    bool Activo
);
