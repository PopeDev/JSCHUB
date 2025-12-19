# Architecture Rules

## Dependency Rule
**Source Code Dependencies must point only inward, toward higher-level policies.**

```
Domain (Core)
  ^
  |
Application (Use Cases)
  ^
  |
Infrastructure (Db, Ext APIs)  <-- Presentation (UI)
```

## Layer Definitions

### Domain
- **Contains**: Entities, Enums, Value Objects, Repository Interfaces, Domain Exceptions.
- **Dependencies**: None (Pure C#).
- **Prohibited**: EF Core attributes (unless absolutely necessary for mapping), External libraries.

### Application
- **Contains**: DTOs, Interfaces (Services), Command/Query Handlers, Validators (FluentValidation), AutoMapper Profiles.
- **Dependencies**: Domain.

### Infrastructure
- **Contains**: DbContext, Repository Implementations, External Service Clients (SendGrid, etc.).
- **Dependencies**: Application, Domain.

### Presentation (Web/API)
- **Contains**: Blazor Pages, Controllers, Middleware.
- **Dependencies**: Application, Infrastructure (only for DI registration).

## Common Patterns
- **Repository**: Abstract data access behind interfaces defined in Domain.
- **Unit of Work**: Manage transactions atomically.
- **Result Pattern**: Return objects indicating success/failure instead of throwing exceptions for logic errors.
