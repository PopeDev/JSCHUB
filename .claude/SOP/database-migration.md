# SOP: Migración de Base de Datos

## Objetivo
Procedimiento para gestionar migraciones de Entity Framework Core con PostgreSQL.

## Pre-requisitos
- Conexión a PostgreSQL configurada en `appsettings.json`
- EF Core Tools instalado: `dotnet tool install --global dotnet-ef`

## Comandos Básicos

### Listar Migraciones
```bash
dotnet ef migrations list
```

### Crear Nueva Migración
```bash
dotnet ef migrations add NombreDescriptivoDeMigracion
```

### Aplicar Migraciones
```bash
dotnet ef database update
```

### Revertir a Migración Específica
```bash
dotnet ef database update NombreMigracionAnterior
```

### Eliminar Última Migración (no aplicada)
```bash
dotnet ef migrations remove
```

### Generar Script SQL
```bash
dotnet ef migrations script -o migration.sql
```

## Procedimiento Completo

### 1. Modificar Entidades o Configuraciones

```csharp
// Ejemplo: Agregar nueva propiedad
public class Usuario
{
    public Guid Id { get; set; }
    public string Nombre { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty; // Nueva
}
```

### 2. Actualizar Configuración EF (si necesario)

```csharp
public class UsuarioConfiguration : IEntityTypeConfiguration<Usuario>
{
    public void Configure(EntityTypeBuilder<Usuario> builder)
    {
        builder.Property(x => x.Email)
               .HasMaxLength(255)
               .IsRequired();
        
        builder.HasIndex(x => x.Email).IsUnique();
    }
}
```

### 3. Generar Migración

```bash
dotnet ef migrations add AddEmailToUsuario
```

### 4. Revisar Migración Generada

Ubicación: `Infrastructure/Data/Migrations/`

```csharp
// Verificar que el Up() y Down() son correctos
public partial class AddEmailToUsuario : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.AddColumn<string>(
            name: "Email",
            table: "usuarios",
            type: "character varying(255)",
            maxLength: 255,
            nullable: false,
            defaultValue: "");
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropColumn(
            name: "Email",
            table: "usuarios");
    }
}
```

### 5. Aplicar en Desarrollo

```bash
dotnet ef database update
```

### 6. Verificar en Base de Datos

```sql
-- Verificar estructura
\d usuarios

-- Verificar datos
SELECT * FROM usuarios LIMIT 5;
```

## Migraciones en Producción

### Generar Script Idempotente
```bash
dotnet ef migrations script --idempotent -o deploy-migration.sql
```

### Backup Previo
```bash
pg_dump -h host -U user -d database > backup_$(date +%Y%m%d).sql
```

### Aplicar con Transacción
```sql
BEGIN;
-- Script de migración
COMMIT;
```

## Checklist

```markdown
### Pre-migración
- [ ] Backup de base de datos (producción)
- [ ] Cambios en entidades completados
- [ ] Configuraciones EF actualizadas

### Migración
- [ ] Migración generada
- [ ] Código Up() revisado
- [ ] Código Down() revisado
- [ ] Sin pérdida de datos

### Post-migración
- [ ] Migración aplicada
- [ ] Estructura verificada
- [ ] Datos verificados
- [ ] Aplicación probada
```

## Solución de Problemas

### Error: No se puede conectar a la BD
```bash
# Verificar connection string
dotnet user-secrets list
```

### Error: Migración pendiente en conflicto
```bash
# Revertir y regenerar
dotnet ef database update MigracionAnterior
dotnet ef migrations remove
```
