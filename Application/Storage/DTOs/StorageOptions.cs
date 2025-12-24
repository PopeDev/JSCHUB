namespace JSCHUB.Application.Storage.DTOs;

/// <summary>
/// Opciones de configuración para el sistema de almacenamiento.
/// </summary>
public sealed class StorageOptions
{
    /// <summary>
    /// Nombre de la sección en appsettings.json.
    /// </summary>
    public const string SectionName = "Storage";

    /// <summary>
    /// Ruta raíz del almacenamiento en disco local.
    /// </summary>
    public string RootPath { get; set; } = "./storage";

    /// <summary>
    /// Tamaño máximo de archivo permitido en bytes.
    /// Por defecto: 100 MB.
    /// </summary>
    public long MaxFileSizeBytes { get; set; } = 100 * 1024 * 1024;

    /// <summary>
    /// Lista blanca de extensiones permitidas (sin punto).
    /// Si está vacía, se permiten todas las extensiones.
    /// </summary>
    public List<string> AllowedExtensions { get; set; } = new();

    /// <summary>
    /// Extensiones de imagen soportadas para generación de miniaturas.
    /// </summary>
    public List<string> ThumbnailSupportedExtensions { get; set; } = new()
    {
        "jpg", "jpeg", "png", "gif", "webp", "bmp"
    };

    /// <summary>
    /// Tamaño por defecto de las miniaturas en píxeles.
    /// </summary>
    public int DefaultThumbnailSize { get; set; } = 128;

    /// <summary>
    /// Nombre de la carpeta de caché de miniaturas.
    /// </summary>
    public string ThumbnailCacheFolder { get; set; } = ".thumbnails";

    /// <summary>
    /// Habilitar caché de miniaturas en disco.
    /// </summary>
    public bool EnableThumbnailCache { get; set; } = true;
}
