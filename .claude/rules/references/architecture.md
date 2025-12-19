# Arquitectura del Proyecto

## Clean Architecture

```
┌─────────────────────────────────────────┐
│              Presentation               │
│    (Blazor Pages, Components, API)      │
├─────────────────────────────────────────┤
│              Application                │
│   (Services, DTOs, Validators, CQRS)    │
├─────────────────────────────────────────┤
│               Domain                    │
│  (Entities, Value Objects, Interfaces)  │
├─────────────────────────────────────────┤
│            Infrastructure               │
│ (DbContext, Repositories, External API) │
└─────────────────────────────────────────┘
```

## Flujo de Dependencias

```
Domain ← Application ← Infrastructure
                    ← Presentation
```

**Regla de Oro**: Las capas internas NO conocen las capas externas.

## Capas

### Domain
```
Domain/
├── Entities/           # Entidades de negocio
│   └── User.cs
├── ValueObjects/       # Objetos de valor
│   └── Email.cs
├── Enums/              # Enumeraciones
│   └── UserStatus.cs
└── Interfaces/         # Contratos
    └── IUserRepository.cs
```

### Application
```
Application/
├── DTOs/               # Data Transfer Objects
│   ├── UserDto.cs
│   └── CreateUserDto.cs
├── Interfaces/         # Contratos de servicios
│   └── IUserService.cs
├── Services/           # Implementación de lógica
│   └── UserService.cs
├── Validators/         # FluentValidation
│   └── CreateUserValidator.cs
└── Mappings/           # AutoMapper profiles
    └── UserProfile.cs
```

### Infrastructure
```
Infrastructure/
├── Data/
│   ├── ApplicationDbContext.cs
│   └── Configurations/
│       └── UserConfiguration.cs
├── Repositories/
│   └── UserRepository.cs
└── Services/
    └── EmailService.cs
```

### Presentation (Blazor)
```
JSCHUB/
├── Components/
│   ├── Layout/
│   └── Pages/
├── wwwroot/
└── Program.cs
```

## Patrones

### Repository Pattern
```csharp
// Domain/Interfaces
public interface IUserRepository
{
    Task<User?> GetByIdAsync(Guid id);
    Task<IEnumerable<User>> GetAllAsync();
    Task AddAsync(User user);
    Task UpdateAsync(User user);
    Task DeleteAsync(Guid id);
}

// Infrastructure/Repositories
public class UserRepository : IUserRepository
{
    private readonly ApplicationDbContext _context;
    // Implementación...
}
```

### Unit of Work
```csharp
public interface IUnitOfWork
{
    IUserRepository Users { get; }
    Task<int> SaveChangesAsync();
}
```

### Dependency Injection
```csharp
// Program.cs
builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();
```
