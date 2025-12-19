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
├── Components/
│   ├── Layout/          # Layouts compartidos
│   └── Pages/           # Páginas de la aplicación
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
