---
description: Add a new entity to the Domain and set up the full stack
---

# Add New Entity Workflow

1. **Create Entity in Domain**
   - Create `Domain/Entities/{EntityName}.cs`
   - Define properties and ensure it inherits from `BaseEntity` (if exists).

2. **Create Repository Interface**
   - Create `Domain/Interfaces/I{EntityName}Repository.cs`

3. **Configure Infrastructure**
   - Add `DbSet<{EntityName}>` to `ApplicationDbContext`.
   - Create `Infrastructure/Data/Configurations/{EntityName}Configuration.cs`.
   - Implement `Infrastructure/Repositories/{EntityName}Repository.cs`.

4. **Create Application Layer Artifacts**
   - Create DTOs: `Application/DTOs/{EntityName}/{EntityName}Dto.cs`, `Create{EntityName}Dto.cs`.
   - Create Validator: `Application/Validators/Create{EntityName}Validator.cs`.
   - Create Service Interface: `Application/Interfaces/I{EntityName}Service.cs`.
   - Create Service Implementation: `Application/Services/{EntityName}Service.cs`.

5. **Register Services**
   - Update `Program.cs` (or DependencyInjection container) to register the Repository and Service.

6. **Create Database Migration**
   // turbo
   - Run command: `dotnet ef migrations add Add{EntityName}`
   - Run command: `dotnet ef database update`
