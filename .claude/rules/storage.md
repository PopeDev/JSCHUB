# Storage Layer - Reglas y Arquitectura

## Principios de Diseño

1. **Desacoplamiento Total**: La UI y la lógica de negocio NUNCA deben depender de la implementación concreta del storage
2. **Interfaz Única**: `IFileStorage` es el contrato que deben cumplir todas las implementaciones
3. **Rutas Lógicas**: El sistema trabaja con rutas lógicas relativas, nunca con rutas físicas absolutas
4. **Seguridad por Defecto**: Path traversal bloqueado, extensiones validadas, tamaños limitados

## Estructura de Archivos

```
Application/Storage/
├── DTOs/
│   ├── StorageItemDto.cs      # Item de storage (archivo o carpeta)
│   ├── ThumbnailResult.cs     # Resultado de miniatura
│   ├── StorageOptions.cs      # Opciones de configuración
│   ├── CreateFolderRequest.cs # Request para crear carpeta
│   ├── RenameRequest.cs       # Request para renombrar
│   └── MoveRequest.cs         # Request para mover
├── Exceptions/
│   └── StorageException.cs    # Excepciones personalizadas
└── Interfaces/
    └── IFileStorage.cs        # Interfaz principal

Infrastructure/Storage/
└── LocalDiskFileStorage.cs    # Implementación para disco local

Infrastructure/Api/
└── FilesEndpoints.cs          # API HTTP (Minimal APIs)
```

## Interfaz IFileStorage

```csharp
public interface IFileStorage
{
    Task<IReadOnlyList<StorageItemDto>> ListAsync(string? path, CancellationToken ct);
    Task CreateFolderAsync(string path, CancellationToken ct);
    Task UploadAsync(string folderPath, string fileName, Stream contentStream, string? contentType, CancellationToken ct);
    Task<(Stream Stream, string ContentType, string FileName)> OpenReadAsync(string path, CancellationToken ct);
    Task DeleteAsync(string path, bool recursive, CancellationToken ct);
    Task RenameAsync(string path, string newName, CancellationToken ct);
    Task MoveAsync(string sourcePath, string destinationFolderPath, CancellationToken ct);
    Task<ThumbnailResult?> GetThumbnailAsync(string path, int size, CancellationToken ct);
    Task<bool> ExistsAsync(string path, CancellationToken ct);
    Task<StorageItemDto?> GetItemAsync(string path, CancellationToken ct);
}
```

## Excepciones

| Excepción | Cuando se lanza |
|-----------|-----------------|
| `PathTraversalException` | Intento de acceder fuera del root |
| `StorageItemNotFoundException` | Item no existe |
| `StorageItemExistsException` | Item ya existe (conflicto) |
| `FileSizeExceededException` | Archivo muy grande |
| `ExtensionNotAllowedException` | Extensión no permitida |
| `InvalidFileNameException` | Nombre de archivo inválido |
| `DirectoryNotEmptyException` | Carpeta no vacía (sin recursive) |

## Configuración

```json
{
  "Storage": {
    "RootPath": "./storage",           // Carpeta raíz
    "MaxFileSizeBytes": 104857600,     // 100 MB
    "AllowedExtensions": [],           // Vacío = todas permitidas
    "ThumbnailSupportedExtensions": ["jpg", "jpeg", "png", "gif", "webp", "bmp"],
    "DefaultThumbnailSize": 128,
    "ThumbnailCacheFolder": ".thumbnails",
    "EnableThumbnailCache": true
  }
}
```

## Implementación Futura (S3/Azure Blob)

Para implementar un nuevo storage (ej: S3):

1. Crear `S3FileStorage : IFileStorage`
2. Implementar todos los métodos de la interfaz
3. Registrar en DI: `services.AddScoped<IFileStorage, S3FileStorage>()`
4. Agregar configuración específica (credentials, bucket, etc.)

La UI y los endpoints NO necesitan cambios.

## API Endpoints

Todos los endpoints están bajo `/api/files`:

- Los endpoints son la ÚNICA API HTTP del proyecto
- Devuelven solo rutas lógicas, nunca físicas
- Errores mapeados a códigos HTTP apropiados (400, 403, 404, 409, 413, 500)

## Buenas Prácticas

1. **No exponer rutas físicas**: Solo usar rutas lógicas en la API y UI
2. **Streaming para uploads/downloads**: No cargar archivos completos en memoria
3. **Validar en el storage**: No confiar en validaciones de capas superiores
4. **Logs de operaciones**: Registrar creates, deletes, renames
5. **Miniaturas cacheadas**: Evitar regeneración innecesaria
