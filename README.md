# JSCHUB - Sistema de Recordatorios y Renovaciones

Sistema interno de backoffice para gestionar renovaciones y recordatorios de forma genérica.

## 🚀 Inicio Rápido

### Requisitos
- .NET 10.0 SDK
- PostgreSQL 14+

### Configuración

1. **Clonar el repositorio**
```bash
git clone <repo-url>
cd JSCHUB
```

2. **Configurar la base de datos**

Editar `appsettings.json`:
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Host=localhost;Port=5432;Database=JSCHUB;Username=postgres;Password=tu_password"
  }
}
```

3. **Restaurar paquetes**
```bash
cd JSCHUB
dotnet restore
```

4. **Aplicar migraciones**
```bash
dotnet ef database update
```

5. **Ejecutar la aplicación**
```bash
dotnet run
```

La aplicación estará disponible en `https://localhost:5001`

## 🐳 Docker (Opcional)

```bash
docker-compose up -d
```

## 📱 Funcionalidades

### Monitor de Alertas (`/backoffice/monitor`)
- Vista de alertas por severidad
- Contadores en tiempo real
- Acciones rápidas: Reconocer, Posponer, Completar

### Gestión de Recordatorios (`/backoffice/reminders`)
- CRUD completo
- Filtros por categoría, estado, etiquetas
- Vista de próximas ocurrencias

### Formulario de Creación (`/backoffice/reminders/new`)
- Eventos únicos o recurrentes
- Frecuencias: Semanal, Mensual, Trimestral, Anual, Personalizado
- Metadata configurable (key-value)

## 🗄️ Modelo de Datos

### ReminderItem
- Entidad genérica para cualquier tipo de recordatorio
- Metadata JSON para datos específicos (dominio, proveedor, etc.)
- Soporte de recurrencia configurable

### Alert
- Generadas automáticamente por el scheduler
- Estados: Open, Acknowledged, Snoozed, Resolved
- Severidad calculada dinámicamente

### AuditLog
- Historial de cambios completo
- Tracking de acciones por usuario

## ⚙️ Arquitectura

```
JSCHUB/
├── Domain/              # Entidades, Enums, Interfaces
├── Application/         # DTOs, Services, Validators
├── Infrastructure/      # DbContext, Repositories, Background Services
└── Components/          # Blazor UI (MudBlazor)
```

## 🔧 Stack Tecnológico

| Componente | Tecnología |
|------------|------------|
| Framework | .NET 10.0 |
| UI | Blazor Server + MudBlazor |
| Base de Datos | PostgreSQL |
| ORM | Entity Framework Core |
| Validación | FluentValidation |

## 📝 Licencia

Uso interno - Todos los derechos reservados.
