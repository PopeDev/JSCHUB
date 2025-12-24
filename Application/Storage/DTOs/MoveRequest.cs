namespace JSCHUB.Application.Storage.DTOs;

/// <summary>
/// Request para mover un archivo o carpeta.
/// </summary>
public sealed record MoveRequest
{
    /// <summary>
    /// Ruta actual del item.
    /// </summary>
    public required string SourcePath { get; init; }

    /// <summary>
    /// Ruta de la carpeta destino.
    /// </summary>
    public required string DestinationFolderPath { get; init; }
}
