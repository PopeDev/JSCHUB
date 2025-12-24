using System.Security.Cryptography;
using JSCHUB.Application.Storage.DTOs;
using JSCHUB.Application.Storage.Exceptions;
using JSCHUB.Application.Storage.Interfaces;
using Microsoft.AspNetCore.StaticFiles;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Processing;

namespace JSCHUB.Infrastructure.Storage;

/// <summary>
/// Implementación de IFileStorage para almacenamiento en disco local.
/// </summary>
public sealed class LocalDiskFileStorage : IFileStorage
{
    private readonly StorageOptions _options;
    private readonly ILogger<LocalDiskFileStorage> _logger;
    private readonly string _rootPath;
    private readonly string _thumbnailCachePath;
    private readonly FileExtensionContentTypeProvider _contentTypeProvider;
    private static readonly HashSet<string> _thumbnailExtensions = new(StringComparer.OrdinalIgnoreCase)
    {
        ".jpg", ".jpeg", ".png", ".gif", ".webp", ".bmp"
    };

    public LocalDiskFileStorage(
        IOptions<StorageOptions> options,
        ILogger<LocalDiskFileStorage> logger)
    {
        _options = options.Value;
        _logger = logger;
        _contentTypeProvider = new FileExtensionContentTypeProvider();

        // Normalizar y crear directorio raíz
        _rootPath = Path.GetFullPath(_options.RootPath);
        if (!Directory.Exists(_rootPath))
        {
            Directory.CreateDirectory(_rootPath);
            _logger.LogInformation("Directorio raíz de storage creado: {RootPath}", _rootPath);
        }

        // Directorio de caché de miniaturas
        _thumbnailCachePath = Path.Combine(_rootPath, _options.ThumbnailCacheFolder);
        if (_options.EnableThumbnailCache && !Directory.Exists(_thumbnailCachePath))
        {
            Directory.CreateDirectory(_thumbnailCachePath);
        }
    }

    public async Task<IReadOnlyList<StorageItemDto>> ListAsync(string? path, CancellationToken ct = default)
    {
        var fullPath = ResolvePath(path ?? string.Empty);

        if (!Directory.Exists(fullPath))
        {
            throw new StorageItemNotFoundException(path ?? "/");
        }

        var items = new List<StorageItemDto>();

        // Obtener carpetas primero
        var directories = Directory.GetDirectories(fullPath)
            .Where(d => !Path.GetFileName(d).StartsWith('.')) // Excluir carpetas ocultas
            .OrderBy(d => Path.GetFileName(d), StringComparer.OrdinalIgnoreCase);

        foreach (var dir in directories)
        {
            ct.ThrowIfCancellationRequested();
            var dirInfo = new DirectoryInfo(dir);
            var relativePath = GetRelativePath(dir);

            items.Add(new StorageItemDto
            {
                Name = dirInfo.Name,
                Path = relativePath,
                IsDirectory = true,
                SizeBytes = null,
                LastModifiedUtc = dirInfo.LastWriteTimeUtc,
                ContentType = null,
                HasThumbnail = false,
                Extension = null
            });
        }

        // Obtener archivos
        var files = Directory.GetFiles(fullPath)
            .Where(f => !Path.GetFileName(f).StartsWith('.')) // Excluir archivos ocultos
            .OrderBy(f => Path.GetFileName(f), StringComparer.OrdinalIgnoreCase);

        foreach (var file in files)
        {
            ct.ThrowIfCancellationRequested();
            var fileInfo = new FileInfo(file);
            var relativePath = GetRelativePath(file);
            var extension = fileInfo.Extension.TrimStart('.').ToLowerInvariant();

            items.Add(new StorageItemDto
            {
                Name = fileInfo.Name,
                Path = relativePath,
                IsDirectory = false,
                SizeBytes = fileInfo.Length,
                LastModifiedUtc = fileInfo.LastWriteTimeUtc,
                ContentType = GetContentType(fileInfo.Name),
                HasThumbnail = IsThumbnailSupported(fileInfo.Extension),
                Extension = string.IsNullOrEmpty(extension) ? null : extension
            });
        }

        return items;
    }

    public Task CreateFolderAsync(string path, CancellationToken ct = default)
    {
        ValidatePath(path);
        var fullPath = ResolvePath(path);

        if (Directory.Exists(fullPath) || File.Exists(fullPath))
        {
            throw new StorageItemExistsException(path);
        }

        Directory.CreateDirectory(fullPath);
        _logger.LogInformation("Carpeta creada: {Path}", path);

        return Task.CompletedTask;
    }

    public async Task UploadAsync(string folderPath, string fileName, Stream contentStream, string? contentType, CancellationToken ct = default)
    {
        ValidateFileName(fileName);
        ValidateExtension(fileName);

        var targetFolder = ResolvePath(folderPath ?? string.Empty);

        if (!Directory.Exists(targetFolder))
        {
            throw new StorageItemNotFoundException(folderPath ?? "/");
        }

        var fullPath = Path.Combine(targetFolder, fileName);
        ValidatePathSecurity(fullPath);

        if (File.Exists(fullPath))
        {
            throw new StorageItemExistsException(Path.Combine(folderPath ?? "", fileName));
        }

        // Streaming directo a disco
        await using var fileStream = new FileStream(
            fullPath,
            FileMode.Create,
            FileAccess.Write,
            FileShare.None,
            bufferSize: 81920,
            useAsync: true);

        long totalBytesWritten = 0;
        var buffer = new byte[81920];
        int bytesRead;

        while ((bytesRead = await contentStream.ReadAsync(buffer, ct)) > 0)
        {
            totalBytesWritten += bytesRead;

            if (totalBytesWritten > _options.MaxFileSizeBytes)
            {
                // Cerrar y eliminar archivo parcial
                await fileStream.DisposeAsync();
                File.Delete(fullPath);
                throw new FileSizeExceededException(totalBytesWritten, _options.MaxFileSizeBytes);
            }

            await fileStream.WriteAsync(buffer.AsMemory(0, bytesRead), ct);
        }

        _logger.LogInformation("Archivo subido: {Path} ({Size} bytes)",
            Path.Combine(folderPath ?? "", fileName), totalBytesWritten);
    }

    public Task<(Stream Stream, string ContentType, string FileName)> OpenReadAsync(string path, CancellationToken ct = default)
    {
        var fullPath = ResolvePath(path);

        if (!File.Exists(fullPath))
        {
            throw new StorageItemNotFoundException(path);
        }

        var stream = new FileStream(
            fullPath,
            FileMode.Open,
            FileAccess.Read,
            FileShare.Read,
            bufferSize: 81920,
            useAsync: true);

        var fileName = Path.GetFileName(fullPath);
        var contentType = GetContentType(fileName);

        return Task.FromResult((Stream: (Stream)stream, ContentType: contentType, FileName: fileName));
    }

    public Task DeleteAsync(string path, bool recursive = false, CancellationToken ct = default)
    {
        var fullPath = ResolvePath(path);

        if (Directory.Exists(fullPath))
        {
            if (!recursive)
            {
                var hasContent = Directory.EnumerateFileSystemEntries(fullPath).Any();
                if (hasContent)
                {
                    throw new DirectoryNotEmptyException(path);
                }
            }

            Directory.Delete(fullPath, recursive);
            _logger.LogInformation("Carpeta eliminada: {Path} (recursivo: {Recursive})", path, recursive);
        }
        else if (File.Exists(fullPath))
        {
            File.Delete(fullPath);

            // Eliminar miniatura en caché si existe
            DeleteCachedThumbnails(path);

            _logger.LogInformation("Archivo eliminado: {Path}", path);
        }
        else
        {
            throw new StorageItemNotFoundException(path);
        }

        return Task.CompletedTask;
    }

    public Task RenameAsync(string path, string newName, CancellationToken ct = default)
    {
        ValidateFileName(newName);
        var fullPath = ResolvePath(path);

        if (!File.Exists(fullPath) && !Directory.Exists(fullPath))
        {
            throw new StorageItemNotFoundException(path);
        }

        var parentDir = Path.GetDirectoryName(fullPath)!;
        var newFullPath = Path.Combine(parentDir, newName);

        // Verificar que el nuevo path no salga del root
        ValidatePathSecurity(newFullPath);

        if (File.Exists(newFullPath) || Directory.Exists(newFullPath))
        {
            var newRelativePath = GetRelativePath(newFullPath);
            throw new StorageItemExistsException(newRelativePath);
        }

        if (Directory.Exists(fullPath))
        {
            Directory.Move(fullPath, newFullPath);
        }
        else
        {
            File.Move(fullPath, newFullPath);
            // Eliminar miniaturas del archivo original
            DeleteCachedThumbnails(path);
        }

        _logger.LogInformation("Item renombrado: {OldPath} -> {NewName}", path, newName);

        return Task.CompletedTask;
    }

    public Task MoveAsync(string sourcePath, string destinationFolderPath, CancellationToken ct = default)
    {
        var sourceFullPath = ResolvePath(sourcePath);
        var destFolderFullPath = ResolvePath(destinationFolderPath ?? string.Empty);

        if (!File.Exists(sourceFullPath) && !Directory.Exists(sourceFullPath))
        {
            throw new StorageItemNotFoundException(sourcePath);
        }

        if (!Directory.Exists(destFolderFullPath))
        {
            throw new StorageItemNotFoundException(destinationFolderPath ?? "/");
        }

        var itemName = Path.GetFileName(sourceFullPath);
        var destFullPath = Path.Combine(destFolderFullPath, itemName);

        ValidatePathSecurity(destFullPath);

        if (File.Exists(destFullPath) || Directory.Exists(destFullPath))
        {
            var destRelativePath = GetRelativePath(destFullPath);
            throw new StorageItemExistsException(destRelativePath);
        }

        if (Directory.Exists(sourceFullPath))
        {
            Directory.Move(sourceFullPath, destFullPath);
        }
        else
        {
            File.Move(sourceFullPath, destFullPath);
            DeleteCachedThumbnails(sourcePath);
        }

        _logger.LogInformation("Item movido: {Source} -> {Destination}", sourcePath, destinationFolderPath);

        return Task.CompletedTask;
    }

    public async Task<ThumbnailResult?> GetThumbnailAsync(string path, int size, CancellationToken ct = default)
    {
        var fullPath = ResolvePath(path);

        if (!File.Exists(fullPath))
        {
            throw new StorageItemNotFoundException(path);
        }

        var extension = Path.GetExtension(fullPath);
        if (!IsThumbnailSupported(extension))
        {
            return null;
        }

        // Buscar en caché
        if (_options.EnableThumbnailCache)
        {
            var cachedThumbnail = await GetCachedThumbnailAsync(path, size, ct);
            if (cachedThumbnail != null)
            {
                return cachedThumbnail;
            }
        }

        // Generar miniatura
        try
        {
            using var image = await Image.LoadAsync(fullPath, ct);

            // Calcular dimensiones manteniendo aspecto
            var (newWidth, newHeight) = CalculateThumbnailSize(image.Width, image.Height, size);

            image.Mutate(x => x.Resize(newWidth, newHeight));

            var memoryStream = new MemoryStream();
            await image.SaveAsJpegAsync(memoryStream, ct);
            memoryStream.Position = 0;

            // Guardar en caché
            if (_options.EnableThumbnailCache)
            {
                await CacheThumbnailAsync(path, size, memoryStream, ct);
                memoryStream.Position = 0;
            }

            return new ThumbnailResult
            {
                Data = memoryStream,
                ContentType = "image/jpeg",
                Width = newWidth,
                Height = newHeight
            };
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Error generando miniatura para: {Path}", path);
            return null;
        }
    }

    public Task<bool> ExistsAsync(string path, CancellationToken ct = default)
    {
        var fullPath = ResolvePath(path);
        var exists = File.Exists(fullPath) || Directory.Exists(fullPath);
        return Task.FromResult(exists);
    }

    public Task<StorageItemDto?> GetItemAsync(string path, CancellationToken ct = default)
    {
        var fullPath = ResolvePath(path);

        if (Directory.Exists(fullPath))
        {
            var dirInfo = new DirectoryInfo(fullPath);
            return Task.FromResult<StorageItemDto?>(new StorageItemDto
            {
                Name = dirInfo.Name,
                Path = path,
                IsDirectory = true,
                SizeBytes = null,
                LastModifiedUtc = dirInfo.LastWriteTimeUtc,
                ContentType = null,
                HasThumbnail = false,
                Extension = null
            });
        }

        if (File.Exists(fullPath))
        {
            var fileInfo = new FileInfo(fullPath);
            var extension = fileInfo.Extension.TrimStart('.').ToLowerInvariant();

            return Task.FromResult<StorageItemDto?>(new StorageItemDto
            {
                Name = fileInfo.Name,
                Path = path,
                IsDirectory = false,
                SizeBytes = fileInfo.Length,
                LastModifiedUtc = fileInfo.LastWriteTimeUtc,
                ContentType = GetContentType(fileInfo.Name),
                HasThumbnail = IsThumbnailSupported(fileInfo.Extension),
                Extension = string.IsNullOrEmpty(extension) ? null : extension
            });
        }

        return Task.FromResult<StorageItemDto?>(null);
    }

    #region Métodos privados de seguridad y validación

    /// <summary>
    /// Resuelve una ruta lógica a una ruta física completa, validando seguridad.
    /// </summary>
    private string ResolvePath(string relativePath)
    {
        // Normalizar separadores y eliminar caracteres peligrosos
        var normalized = NormalizePath(relativePath);

        // Combinar con root
        var combined = Path.Combine(_rootPath, normalized);
        var fullPath = Path.GetFullPath(combined);

        // Validar que no salga del root
        ValidatePathSecurity(fullPath);

        return fullPath;
    }

    /// <summary>
    /// Normaliza una ruta eliminando caracteres y secuencias peligrosas.
    /// </summary>
    private static string NormalizePath(string path)
    {
        if (string.IsNullOrWhiteSpace(path))
        {
            return string.Empty;
        }

        // Reemplazar backslashes por forward slashes
        path = path.Replace('\\', '/');

        // Eliminar múltiples slashes consecutivas
        while (path.Contains("//"))
        {
            path = path.Replace("//", "/");
        }

        // Eliminar slash inicial y final
        path = path.Trim('/');

        // Eliminar segmentos "." y ".."
        var segments = path.Split('/')
            .Where(s => !string.IsNullOrWhiteSpace(s) && s != "." && s != "..")
            .ToArray();

        return string.Join(Path.DirectorySeparatorChar.ToString(), segments);
    }

    /// <summary>
    /// Valida que la ruta completa esté dentro del directorio raíz.
    /// </summary>
    private void ValidatePathSecurity(string fullPath)
    {
        var normalizedRoot = Path.GetFullPath(_rootPath).TrimEnd(Path.DirectorySeparatorChar) + Path.DirectorySeparatorChar;
        var normalizedPath = Path.GetFullPath(fullPath);

        // Permitir el root exacto o paths dentro del root
        if (!normalizedPath.StartsWith(normalizedRoot, StringComparison.OrdinalIgnoreCase) &&
            !normalizedPath.Equals(normalizedRoot.TrimEnd(Path.DirectorySeparatorChar), StringComparison.OrdinalIgnoreCase))
        {
            _logger.LogWarning("Intento de path traversal bloqueado: {Path}", fullPath);
            throw new PathTraversalException(fullPath);
        }
    }

    /// <summary>
    /// Valida una ruta relativa.
    /// </summary>
    private void ValidatePath(string path)
    {
        if (string.IsNullOrWhiteSpace(path))
        {
            throw new InvalidFileNameException(path ?? "", "La ruta no puede estar vacía.");
        }

        // Verificar caracteres inválidos
        var invalidChars = Path.GetInvalidPathChars();
        if (path.IndexOfAny(invalidChars) >= 0)
        {
            throw new InvalidFileNameException(path, "La ruta contiene caracteres inválidos.");
        }
    }

    /// <summary>
    /// Valida un nombre de archivo.
    /// </summary>
    private static void ValidateFileName(string fileName)
    {
        if (string.IsNullOrWhiteSpace(fileName))
        {
            throw new InvalidFileNameException(fileName ?? "", "El nombre no puede estar vacío.");
        }

        var invalidChars = Path.GetInvalidFileNameChars();
        if (fileName.IndexOfAny(invalidChars) >= 0)
        {
            throw new InvalidFileNameException(fileName, "El nombre contiene caracteres inválidos.");
        }

        if (fileName.StartsWith('.'))
        {
            throw new InvalidFileNameException(fileName, "El nombre no puede comenzar con punto.");
        }

        // Nombres reservados en Windows
        var reservedNames = new[] { "CON", "PRN", "AUX", "NUL", "COM1", "COM2", "COM3", "COM4",
            "COM5", "COM6", "COM7", "COM8", "COM9", "LPT1", "LPT2", "LPT3", "LPT4",
            "LPT5", "LPT6", "LPT7", "LPT8", "LPT9" };

        var nameWithoutExt = Path.GetFileNameWithoutExtension(fileName).ToUpperInvariant();
        if (reservedNames.Contains(nameWithoutExt))
        {
            throw new InvalidFileNameException(fileName, "Nombre de archivo reservado.");
        }
    }

    /// <summary>
    /// Valida la extensión del archivo contra la lista blanca.
    /// </summary>
    private void ValidateExtension(string fileName)
    {
        if (_options.AllowedExtensions.Count == 0)
        {
            return; // Sin restricciones
        }

        var extension = Path.GetExtension(fileName).TrimStart('.').ToLowerInvariant();

        if (string.IsNullOrEmpty(extension) ||
            !_options.AllowedExtensions.Any(e => e.Equals(extension, StringComparison.OrdinalIgnoreCase)))
        {
            throw new ExtensionNotAllowedException(extension, _options.AllowedExtensions);
        }
    }

    #endregion

    #region Métodos privados de utilidad

    /// <summary>
    /// Obtiene la ruta relativa desde el root.
    /// </summary>
    private string GetRelativePath(string fullPath)
    {
        var relative = Path.GetRelativePath(_rootPath, fullPath);
        return relative.Replace(Path.DirectorySeparatorChar, '/');
    }

    /// <summary>
    /// Obtiene el content type de un archivo.
    /// </summary>
    private string GetContentType(string fileName)
    {
        if (!_contentTypeProvider.TryGetContentType(fileName, out var contentType))
        {
            contentType = "application/octet-stream";
        }
        return contentType;
    }

    /// <summary>
    /// Verifica si la extensión soporta miniaturas.
    /// </summary>
    private static bool IsThumbnailSupported(string extension)
    {
        return _thumbnailExtensions.Contains(extension);
    }

    /// <summary>
    /// Calcula el tamaño de la miniatura manteniendo el aspecto.
    /// </summary>
    private static (int Width, int Height) CalculateThumbnailSize(int originalWidth, int originalHeight, int maxSize)
    {
        if (originalWidth <= maxSize && originalHeight <= maxSize)
        {
            return (originalWidth, originalHeight);
        }

        double ratio;
        if (originalWidth > originalHeight)
        {
            ratio = (double)maxSize / originalWidth;
        }
        else
        {
            ratio = (double)maxSize / originalHeight;
        }

        return ((int)(originalWidth * ratio), (int)(originalHeight * ratio));
    }

    #endregion

    #region Métodos privados de caché de miniaturas

    /// <summary>
    /// Genera un nombre único para la miniatura en caché.
    /// </summary>
    private string GetThumbnailCacheKey(string path, int size)
    {
        using var sha256 = SHA256.Create();
        var hash = sha256.ComputeHash(System.Text.Encoding.UTF8.GetBytes($"{path}_{size}"));
        return Convert.ToHexString(hash).ToLowerInvariant();
    }

    /// <summary>
    /// Obtiene una miniatura de la caché si existe y es válida.
    /// </summary>
    private async Task<ThumbnailResult?> GetCachedThumbnailAsync(string path, int size, CancellationToken ct)
    {
        var cacheKey = GetThumbnailCacheKey(path, size);
        var cachePath = Path.Combine(_thumbnailCachePath, $"{cacheKey}.jpg");

        if (!File.Exists(cachePath))
        {
            return null;
        }

        // Verificar que el original no sea más nuevo que la caché
        var originalPath = ResolvePath(path);
        var originalModified = File.GetLastWriteTimeUtc(originalPath);
        var cacheModified = File.GetLastWriteTimeUtc(cachePath);

        if (originalModified > cacheModified)
        {
            // Caché obsoleta
            File.Delete(cachePath);
            return null;
        }

        var memoryStream = new MemoryStream();
        await using var fileStream = File.OpenRead(cachePath);
        await fileStream.CopyToAsync(memoryStream, ct);
        memoryStream.Position = 0;

        // Obtener dimensiones de la imagen cacheada
        using var image = await Image.LoadAsync(cachePath, ct);

        return new ThumbnailResult
        {
            Data = memoryStream,
            ContentType = "image/jpeg",
            Width = image.Width,
            Height = image.Height
        };
    }

    /// <summary>
    /// Guarda una miniatura en la caché.
    /// </summary>
    private async Task CacheThumbnailAsync(string path, int size, MemoryStream thumbnailStream, CancellationToken ct)
    {
        try
        {
            var cacheKey = GetThumbnailCacheKey(path, size);
            var cachePath = Path.Combine(_thumbnailCachePath, $"{cacheKey}.jpg");

            thumbnailStream.Position = 0;
            await using var fileStream = File.Create(cachePath);
            await thumbnailStream.CopyToAsync(fileStream, ct);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Error guardando miniatura en caché para: {Path}", path);
        }
    }

    /// <summary>
    /// Elimina miniaturas cacheadas de un archivo.
    /// </summary>
    private void DeleteCachedThumbnails(string path)
    {
        if (!_options.EnableThumbnailCache)
        {
            return;
        }

        try
        {
            // Buscar todas las posibles miniaturas (diferentes tamaños)
            foreach (var size in new[] { 64, 128, 256, 512 })
            {
                var cacheKey = GetThumbnailCacheKey(path, size);
                var cachePath = Path.Combine(_thumbnailCachePath, $"{cacheKey}.jpg");

                if (File.Exists(cachePath))
                {
                    File.Delete(cachePath);
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Error eliminando miniaturas cacheadas para: {Path}", path);
        }
    }

    #endregion
}
