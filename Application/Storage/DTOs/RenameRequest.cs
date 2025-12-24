namespace JSCHUB.Application.Storage.DTOs;

/// <summary>
/// Request para renombrar un archivo o carpeta.
/// </summary>
public sealed record RenameRequest
{
    /// <summary>
    /// Ruta actual del item.
    /// </summary>
    public required string Path { get; init; }

    /// <summary>
    /// Nuevo nombre (solo el nombre, sin ruta).
    /// </summary>
    public required string NewName { get; init; }
}
