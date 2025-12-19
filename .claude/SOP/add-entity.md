# SOP: Agregar Nueva Entidad

## Objetivo
Procedimiento paso a paso para agregar una nueva entidad al sistema siguiendo Clean Architecture.

## Pasos

### 1. Crear Entidad en Domain

```csharp
// Domain/Entities/NuevaEntidad.cs
namespace Domain.Entities;

public class NuevaEntidad
{
    public Guid Id { get; set; }
    public string Nombre { get; set; } = string.Empty;
    public DateTime FechaCreacion { get; set; }
    public bool Activo { get; set; } = true;
}
```

### 2. Crear Interfaz del Repository

```csharp
// Domain/Interfaces/INuevaEntidadRepository.cs
namespace Domain.Interfaces;

public interface INuevaEntidadRepository
{
    Task<NuevaEntidad?> GetByIdAsync(Guid id);
    Task<IEnumerable<NuevaEntidad>> GetAllAsync();
    Task AddAsync(NuevaEntidad entidad);
    Task UpdateAsync(NuevaEntidad entidad);
    Task DeleteAsync(Guid id);
}
```

### 3. Configurar en DbContext

```csharp
// Infrastructure/Data/ApplicationDbContext.cs
public DbSet<NuevaEntidad> NuevasEntidades { get; set; }

// Infrastructure/Data/Configurations/NuevaEntidadConfiguration.cs
public class NuevaEntidadConfiguration : IEntityTypeConfiguration<NuevaEntidad>
{
    public void Configure(EntityTypeBuilder<NuevaEntidad> builder)
    {
        builder.ToTable("nuevas_entidades");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Nombre).HasMaxLength(200).IsRequired();
    }
}
```

### 4. Implementar Repository

```csharp
// Infrastructure/Repositories/NuevaEntidadRepository.cs
public class NuevaEntidadRepository : INuevaEntidadRepository
{
    private readonly ApplicationDbContext _context;
    
    public NuevaEntidadRepository(ApplicationDbContext context)
    {
        _context = context;
    }
    
    // Implementar métodos...
}
```

### 5. Crear DTOs

```csharp
// Application/DTOs/NuevaEntidadDto.cs
public record NuevaEntidadDto(Guid Id, string Nombre, DateTime FechaCreacion);

// Application/DTOs/CreateNuevaEntidadDto.cs
public record CreateNuevaEntidadDto(string Nombre);
```

### 6. Crear Validator

```csharp
// Application/Validators/CreateNuevaEntidadValidator.cs
public class CreateNuevaEntidadValidator : AbstractValidator<CreateNuevaEntidadDto>
{
    public CreateNuevaEntidadValidator()
    {
        RuleFor(x => x.Nombre)
            .NotEmpty().WithMessage("El nombre es requerido")
            .MaximumLength(200).WithMessage("Máximo 200 caracteres");
    }
}
```

### 7. Crear Service

```csharp
// Application/Services/NuevaEntidadService.cs
public class NuevaEntidadService : INuevaEntidadService
{
    private readonly INuevaEntidadRepository _repository;
    // Implementar lógica de negocio...
}
```

### 8. Registrar en DI

```csharp
// Program.cs
builder.Services.AddScoped<INuevaEntidadRepository, NuevaEntidadRepository>();
builder.Services.AddScoped<INuevaEntidadService, NuevaEntidadService>();
```

### 9. Generar Migración

```bash
dotnet ef migrations add AddNuevaEntidad
dotnet ef database update
```

## Checklist

```markdown
- [ ] Entidad creada en Domain
- [ ] Interfaz de Repository definida
- [ ] Configuración EF Core completada
- [ ] Repository implementado
- [ ] DTOs creados
- [ ] Validator implementado
- [ ] Service implementado
- [ ] Servicios registrados en DI
- [ ] Migración generada y aplicada
```
