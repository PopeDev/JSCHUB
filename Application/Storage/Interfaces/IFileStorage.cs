using JSCHUB.Application.Storage.DTOs;

namespace JSCHUB.Application.Storage.Interfaces;

/// <summary>
/// Interfaz para operaciones de almacenamiento de archivos.
/// Permite implementaciones intercambiables (disco local, S3, Azure Blob, etc.).
/// </summary>
public interface IFileStorage
{
    /// <summary>
    /// Lista los items (archivos y carpetas) en una ruta específica.
    /// Devuelve carpetas primero, luego archivos, ambos ordenados alfabéticamente.
    /// </summary>
    /// <param name="path">Ruta lógica relativa al root. Null o vacío para la raíz.</param>
    /// <param name="ct">Token de cancelación.</param>
    /// <returns>Lista de items encontrados.</returns>
    Task<IReadOnlyList<StorageItemDto>> ListAsync(string? path, CancellationToken ct = default);

    /// <summary>
    /// Crea una nueva carpeta en la ruta especificada.
    /// </summary>
    /// <param name="path">Ruta completa de la nueva carpeta.</param>
    /// <param name="ct">Token de cancelación.</param>
    Task CreateFolderAsync(string path, CancellationToken ct = default);

    /// <summary>
    /// Sube un archivo a la carpeta especificada.
    /// </summary>
    /// <param name="folderPath">Ruta de la carpeta destino.</param>
    /// <param name="fileName">Nombre del archivo.</param>
    /// <param name="contentStream">Stream con el contenido del archivo.</param>
    /// <param name="contentType">Tipo MIME del archivo.</param>
    /// <param name="ct">Token de cancelación.</param>
    Task UploadAsync(string folderPath, string fileName, Stream contentStream, string? contentType, CancellationToken ct = default);

    /// <summary>
    /// Abre un archivo para lectura (streaming).
    /// </summary>
    /// <param name="path">Ruta del archivo.</param>
    /// <param name="ct">Token de cancelación.</param>
    /// <returns>Stream del archivo y su tipo de contenido.</returns>
    Task<(Stream Stream, string ContentType, string FileName)> OpenReadAsync(string path, CancellationToken ct = default);

    /// <summary>
    /// Elimina un archivo o carpeta.
    /// </summary>
    /// <param name="path">Ruta del item a eliminar.</param>
    /// <param name="recursive">Si es carpeta, eliminar contenido recursivamente.</param>
    /// <param name="ct">Token de cancelación.</param>
    Task DeleteAsync(string path, bool recursive = false, CancellationToken ct = default);

    /// <summary>
    /// Renombra un archivo o carpeta.
    /// </summary>
    /// <param name="path">Ruta actual del item.</param>
    /// <param name="newName">Nuevo nombre (sin ruta).</param>
    /// <param name="ct">Token de cancelación.</param>
    Task RenameAsync(string path, string newName, CancellationToken ct = default);

    /// <summary>
    /// Mueve un archivo o carpeta a otra ubicación.
    /// </summary>
    /// <param name="sourcePath">Ruta actual del item.</param>
    /// <param name="destinationFolderPath">Ruta de la carpeta destino.</param>
    /// <param name="ct">Token de cancelación.</param>
    Task MoveAsync(string sourcePath, string destinationFolderPath, CancellationToken ct = default);

    /// <summary>
    /// Obtiene una miniatura del archivo si es una imagen.
    /// </summary>
    /// <param name="path">Ruta del archivo.</param>
    /// <param name="size">Tamaño máximo de la miniatura en píxeles.</param>
    /// <param name="ct">Token de cancelación.</param>
    /// <returns>Resultado de la miniatura o null si no es imagen.</returns>
    Task<ThumbnailResult?> GetThumbnailAsync(string path, int size, CancellationToken ct = default);

    /// <summary>
    /// Verifica si un item existe.
    /// </summary>
    /// <param name="path">Ruta del item.</param>
    /// <param name="ct">Token de cancelación.</param>
    /// <returns>True si existe.</returns>
    Task<bool> ExistsAsync(string path, CancellationToken ct = default);

    /// <summary>
    /// Obtiene información de un item específico.
    /// </summary>
    /// <param name="path">Ruta del item.</param>
    /// <param name="ct">Token de cancelación.</param>
    /// <returns>Información del item o null si no existe.</returns>
    Task<StorageItemDto?> GetItemAsync(string path, CancellationToken ct = default);
}
