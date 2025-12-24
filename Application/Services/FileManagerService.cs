using JSCHUB.Application.Storage.DTOs;
using JSCHUB.Application.Storage.Interfaces;

namespace JSCHUB.Application.Services;

/// <summary>
/// Servicio para el gestor de ficheros de la UI.
/// Actúa como fachada sobre IFileStorage y proporciona URLs para recursos.
/// </summary>
public interface IFileManagerService
{
    /// <summary>
    /// Lista los items en una ruta.
    /// </summary>
    Task<IReadOnlyList<StorageItemDto>> ListAsync(string? path, CancellationToken ct = default);

    /// <summary>
    /// Crea una carpeta.
    /// </summary>
    Task CreateFolderAsync(string path, CancellationToken ct = default);

    /// <summary>
    /// Sube un archivo.
    /// </summary>
    Task UploadAsync(string folderPath, string fileName, Stream content, string? contentType, CancellationToken ct = default);

    /// <summary>
    /// Elimina un item.
    /// </summary>
    Task DeleteAsync(string path, bool recursive = false, CancellationToken ct = default);

    /// <summary>
    /// Renombra un item.
    /// </summary>
    Task RenameAsync(string path, string newName, CancellationToken ct = default);

    /// <summary>
    /// Mueve un item.
    /// </summary>
    Task MoveAsync(string sourcePath, string destinationFolderPath, CancellationToken ct = default);

    /// <summary>
    /// Obtiene la URL de descarga para un archivo.
    /// </summary>
    string GetDownloadUrl(string path, bool inline = false);

    /// <summary>
    /// Obtiene la URL de miniatura para un archivo.
    /// </summary>
    string GetThumbnailUrl(string path, int size = 128);

    /// <summary>
    /// Verifica si un item existe.
    /// </summary>
    Task<bool> ExistsAsync(string path, CancellationToken ct = default);

    /// <summary>
    /// Obtiene información de un item.
    /// </summary>
    Task<StorageItemDto?> GetItemAsync(string path, CancellationToken ct = default);
}

/// <summary>
/// Implementación del servicio de gestor de ficheros.
/// </summary>
public sealed class FileManagerService : IFileManagerService
{
    private readonly IFileStorage _storage;

    public FileManagerService(IFileStorage storage)
    {
        _storage = storage;
    }

    public Task<IReadOnlyList<StorageItemDto>> ListAsync(string? path, CancellationToken ct = default)
        => _storage.ListAsync(path, ct);

    public Task CreateFolderAsync(string path, CancellationToken ct = default)
        => _storage.CreateFolderAsync(path, ct);

    public Task UploadAsync(string folderPath, string fileName, Stream content, string? contentType, CancellationToken ct = default)
        => _storage.UploadAsync(folderPath, fileName, content, contentType, ct);

    public Task DeleteAsync(string path, bool recursive = false, CancellationToken ct = default)
        => _storage.DeleteAsync(path, recursive, ct);

    public Task RenameAsync(string path, string newName, CancellationToken ct = default)
        => _storage.RenameAsync(path, newName, ct);

    public Task MoveAsync(string sourcePath, string destinationFolderPath, CancellationToken ct = default)
        => _storage.MoveAsync(sourcePath, destinationFolderPath, ct);

    public string GetDownloadUrl(string path, bool inline = false)
    {
        var encodedPath = Uri.EscapeDataString(path);
        var url = $"/api/files/download?path={encodedPath}";
        if (inline)
        {
            url += "&inline=true";
        }
        return url;
    }

    public string GetThumbnailUrl(string path, int size = 128)
    {
        var encodedPath = Uri.EscapeDataString(path);
        return $"/api/files/thumbnail?path={encodedPath}&size={size}";
    }

    public Task<bool> ExistsAsync(string path, CancellationToken ct = default)
        => _storage.ExistsAsync(path, ct);

    public Task<StorageItemDto?> GetItemAsync(string path, CancellationToken ct = default)
        => _storage.GetItemAsync(path, ct);
}
