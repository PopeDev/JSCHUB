namespace JSCHUB.Application.Storage.DTOs;

/// <summary>
/// Resultado de la generación de una miniatura.
/// </summary>
public sealed record ThumbnailResult
{
    /// <summary>
    /// Stream con los datos de la miniatura.
    /// </summary>
    public required Stream Data { get; init; }

    /// <summary>
    /// Tipo MIME de la miniatura (ej: "image/jpeg").
    /// </summary>
    public required string ContentType { get; init; }

    /// <summary>
    /// Ancho de la miniatura en píxeles.
    /// </summary>
    public int Width { get; init; }

    /// <summary>
    /// Alto de la miniatura en píxeles.
    /// </summary>
    public int Height { get; init; }
}
