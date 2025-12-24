namespace JSCHUB.Application.Storage.Exceptions;

/// <summary>
/// Excepción base para errores del sistema de almacenamiento.
/// </summary>
public class StorageException : Exception
{
    public StorageException(string message) : base(message) { }
    public StorageException(string message, Exception innerException) : base(message, innerException) { }
}

/// <summary>
/// Excepción cuando se detecta un intento de path traversal.
/// </summary>
public class PathTraversalException : StorageException
{
    public PathTraversalException(string attemptedPath)
        : base($"Acceso denegado: intento de path traversal detectado.")
    {
        AttemptedPath = attemptedPath;
    }

    public string AttemptedPath { get; }
}

/// <summary>
/// Excepción cuando un item no se encuentra.
/// </summary>
public class StorageItemNotFoundException : StorageException
{
    public StorageItemNotFoundException(string path)
        : base($"El item no existe: {path}")
    {
        Path = path;
    }

    public string Path { get; }
}

/// <summary>
/// Excepción cuando un item ya existe.
/// </summary>
public class StorageItemExistsException : StorageException
{
    public StorageItemExistsException(string path)
        : base($"El item ya existe: {path}")
    {
        Path = path;
    }

    public string Path { get; }
}

/// <summary>
/// Excepción cuando el tamaño del archivo excede el límite.
/// </summary>
public class FileSizeExceededException : StorageException
{
    public FileSizeExceededException(long fileSize, long maxSize)
        : base($"El tamaño del archivo ({FormatSize(fileSize)}) excede el límite permitido ({FormatSize(maxSize)}).")
    {
        FileSize = fileSize;
        MaxSize = maxSize;
    }

    public long FileSize { get; }
    public long MaxSize { get; }

    private static string FormatSize(long bytes)
    {
        string[] sizes = { "B", "KB", "MB", "GB" };
        int order = 0;
        double size = bytes;
        while (size >= 1024 && order < sizes.Length - 1)
        {
            order++;
            size /= 1024;
        }
        return $"{size:0.##} {sizes[order]}";
    }
}

/// <summary>
/// Excepción cuando la extensión del archivo no está permitida.
/// </summary>
public class ExtensionNotAllowedException : StorageException
{
    public ExtensionNotAllowedException(string extension, IEnumerable<string> allowedExtensions)
        : base($"La extensión '{extension}' no está permitida. Extensiones permitidas: {string.Join(", ", allowedExtensions)}")
    {
        Extension = extension;
        AllowedExtensions = allowedExtensions.ToList();
    }

    public string Extension { get; }
    public List<string> AllowedExtensions { get; }
}

/// <summary>
/// Excepción cuando el nombre del archivo es inválido.
/// </summary>
public class InvalidFileNameException : StorageException
{
    public InvalidFileNameException(string fileName, string reason)
        : base($"Nombre de archivo inválido '{fileName}': {reason}")
    {
        FileName = fileName;
        Reason = reason;
    }

    public string FileName { get; }
    public string Reason { get; }
}

/// <summary>
/// Excepción cuando se intenta eliminar una carpeta no vacía sin el flag recursivo.
/// </summary>
public class DirectoryNotEmptyException : StorageException
{
    public DirectoryNotEmptyException(string path)
        : base($"La carpeta no está vacía. Use la opción recursiva para eliminar: {path}")
    {
        Path = path;
    }

    public string Path { get; }
}
