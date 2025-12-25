namespace JSCHUB.Application.DTOs;

/// <summary>
/// DTO de lectura para CredencialProyecto (sin contraseña)
/// </summary>
public record CredencialProyectoDto(
    Guid Id,
    Guid EnlaceProyectoId,
    string Nombre,
    string Usuario,
    string? Notas,
    bool Activa,
    DateTime? UltimoAcceso,
    string CreadoPor,
    DateTime CreadoEl,
    string ModificadoPor,
    DateTime ModificadoEl,
    // Datos del enlace para contexto
    string? EnlaceTitulo = null,
    string? EnlaceUrl = null
);

/// <summary>
/// DTO de lectura con contraseña descifrada (solo para uso interno)
/// </summary>
public record CredencialProyectoConPasswordDto(
    Guid Id,
    Guid EnlaceProyectoId,
    string Nombre,
    string Usuario,
    string Password,
    string? Notas,
    bool Activa,
    DateTime? UltimoAcceso,
    string CreadoPor,
    DateTime CreadoEl,
    string ModificadoPor,
    DateTime ModificadoEl,
    string? EnlaceTitulo = null,
    string? EnlaceUrl = null
);

/// <summary>
/// DTO para crear una credencial de proyecto
/// </summary>
public record CreateCredencialProyectoDto(
    Guid EnlaceProyectoId,
    string Nombre,
    string Usuario,
    string Password,
    string? Notas
);

/// <summary>
/// DTO para actualizar una credencial de proyecto
/// </summary>
public record UpdateCredencialProyectoDto(
    string Nombre,
    string Usuario,
    string? Password,
    string? Notas,
    bool Activa
);
