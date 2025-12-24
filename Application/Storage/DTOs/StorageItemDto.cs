namespace JSCHUB.Application.Storage.DTOs;

/// <summary>
/// Representa un item del sistema de almacenamiento (archivo o carpeta).
/// </summary>
public sealed record StorageItemDto
{
    /// <summary>
    /// Nombre del item (sin ruta).
    /// </summary>
    public required string Name { get; init; }

    /// <summary>
    /// Ruta lógica relativa al root del storage.
    /// Ejemplo: "documents/reports" o "documents/reports/file.pdf"
    /// </summary>
    public required string Path { get; init; }

    /// <summary>
    /// Indica si es un directorio (true) o archivo (false).
    /// </summary>
    public required bool IsDirectory { get; init; }

    /// <summary>
    /// Tamaño en bytes. Null para directorios.
    /// </summary>
    public long? SizeBytes { get; init; }

    /// <summary>
    /// Fecha de última modificación en UTC.
    /// </summary>
    public required DateTime LastModifiedUtc { get; init; }

    /// <summary>
    /// Tipo MIME del contenido. Null para directorios.
    /// </summary>
    public string? ContentType { get; init; }

    /// <summary>
    /// Indica si el item tiene una miniatura disponible.
    /// </summary>
    public bool HasThumbnail { get; init; }

    /// <summary>
    /// Extensión del archivo (sin punto). Null para directorios.
    /// </summary>
    public string? Extension { get; init; }
}
