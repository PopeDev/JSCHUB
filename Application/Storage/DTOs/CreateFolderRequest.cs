namespace JSCHUB.Application.Storage.DTOs;

/// <summary>
/// Request para crear una nueva carpeta.
/// </summary>
public sealed record CreateFolderRequest
{
    /// <summary>
    /// Ruta donde crear la carpeta.
    /// </summary>
    public required string Path { get; init; }
}
