# JSCHUB - Claude Code Configuration

## 🎯 Proyecto
**JSCHUB** - Aplicación web Blazor Server con .NET 10.0

## 🛠️ Stack Tecnológico

| Categoría | Tecnología |
|-----------|------------|
| **Framework** | .NET 10.0, Blazor Server |
| **UI** | MudBlazor |
| **Validación** | FluentValidation |
| **ORM** | Entity Framework Core |
| **Base de Datos** | PostgreSQL |
| **Logging** | Serilog |
| **Arquitectura** | Clean Architecture (Api/Application/Domain/Infrastructure) |

## 📁 Estructura del Proyecto

```
JSCHUB/
├── Application/
│   ├── DTOs/            # Data Transfer Objects
│   ├── Interfaces/      # Interfaces de servicios
│   ├── Services/        # Servicios de aplicación
│   ├── Storage/         # Sistema de almacenamiento
│   │   ├── DTOs/        # DTOs de storage
│   │   ├── Exceptions/  # Excepciones personalizadas
│   │   └── Interfaces/  # IFileStorage
│   └── Validators/      # FluentValidation
├── Components/
│   ├── Dialogs/         # Diálogos MudBlazor
│   ├── Layout/          # Layouts compartidos
│   └── Pages/           # Páginas de la aplicación
├── Domain/              # Entidades y enums
├── Infrastructure/
│   ├── Api/             # Endpoints HTTP (Minimal APIs)
│   ├── Data/            # DbContext y configuraciones
│   ├── Repositories/    # Implementaciones de repositorios
│   ├── Services/        # Servicios de infraestructura
│   └── Storage/         # Implementación de storage (LocalDisk)
├── Tests/               # Tests unitarios
├── wwwroot/             # Archivos estáticos
├── Program.cs           # Punto de entrada
└── .claude/             # Configuración de Claude
```

## 💻 Comandos Básicos

```bash
# Desarrollo
dotnet restore          # Restaurar dependencias
dotnet build            # Compilar
dotnet run              # Ejecutar en desarrollo
dotnet watch run        # Hot reload

# Base de datos (EF Core)
dotnet ef migrations add <NombreMigracion>
dotnet ef database update
dotnet ef migrations list

# Producción
dotnet publish -c Release -o ./publish
```

## 📐 Convenciones

### Nombrado
- **Clases/Métodos**: `PascalCase` → `UserService`, `GetAllUsers()`
- **Variables/Parámetros**: `camelCase` → `userName`, `userId`
- **Constantes**: `UPPER_SNAKE_CASE` → `MAX_RETRY_COUNT`
- **Archivos Blazor**: `NombreComponente.razor`

### Arquitectura Clean
```
Domain/         → Entidades, Value Objects, Interfaces
Application/    → DTOs, Services, Validators, UseCases
Infrastructure/ → DbContext, Repositories, External Services
Api/Presentation/ → Controllers, Pages, Components
```

## 📚 Documentación Profunda

Consulta `.claude/` para información detallada:

| Directorio | Propósito |
|------------|-----------|
| `agents/` | Roles especializados (security, reviewer) |
| `commands/` | Atajos de flujo (deploy, new-component) |
| `rules/` | Normas de codificación y arquitectura |
| `SOP/` | Procedimientos paso a paso |
| `skills/` | Patrones y plantillas reutilizables |

## ⚠️ Reglas Críticas

1. **Siempre usar MudBlazor** para componentes UI
2. **Validar con FluentValidation** antes de persistir
3. **Repository Pattern** para acceso a datos
4. **Inyección de dependencias** para todos los servicios
5. **Logs estructurados** con Serilog en operaciones críticas
6. **Solo API HTTP para ficheros** - No hay otros endpoints API en el proyecto

## 📂 Sistema de Almacenamiento (Storage)

### Arquitectura
El sistema de storage está diseñado con una interfaz desacoplada (`IFileStorage`) que permite implementaciones intercambiables:

- **LocalDiskFileStorage**: Implementación actual para disco local
- **S3FileStorage**: (Futuro) Para Amazon S3
- **AzureBlobFileStorage**: (Futuro) Para Azure Blob Storage

### Configuración (appsettings.json)
```json
{
  "Storage": {
    "RootPath": "./storage",
    "MaxFileSizeBytes": 104857600,
    "AllowedExtensions": [],
    "ThumbnailSupportedExtensions": ["jpg", "jpeg", "png", "gif", "webp", "bmp"],
    "DefaultThumbnailSize": 128,
    "ThumbnailCacheFolder": ".thumbnails",
    "EnableThumbnailCache": true
  }
}
```

### API Endpoints (Files)
| Método | Endpoint | Descripción |
|--------|----------|-------------|
| GET | `/api/files?path=...` | Listar items |
| POST | `/api/files/folders` | Crear carpeta |
| POST | `/api/files/upload?path=...` | Subir archivo |
| GET | `/api/files/download?path=...` | Descargar archivo |
| GET | `/api/files/thumbnail?path=...&size=...` | Obtener miniatura |
| DELETE | `/api/files?path=...&recursive=...` | Eliminar item |
| POST | `/api/files/rename` | Renombrar item |
| POST | `/api/files/move` | Mover item |

### Seguridad
- **Path Traversal Prevention**: Todas las rutas se validan para evitar acceso fuera del directorio root
- **Extensiones**: Lista blanca opcional de extensiones permitidas
- **Tamaño máximo**: Límite configurable de tamaño de archivo
- **Rutas lógicas**: La API nunca expone rutas físicas del servidor

### UI (FileManager)
Accesible en `/backoffice/files`:
- Navegación por carpetas con breadcrumb
- Upload de múltiples archivos (drag & drop)
- Preview de imágenes con miniaturas
- Operaciones: crear carpeta, renombrar, eliminar, descargar
