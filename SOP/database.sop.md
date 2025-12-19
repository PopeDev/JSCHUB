# Database Operations SOP

**Status**: DRAFT
**Version**: 1.0

## 1. Purpose
This SOP defines the strict procedures for making changes to the database schema and data integrity.

## 2. Scope
Applies to all developers working on the JSCHUB project involving PostgreSQL and Entity Framework Core.

## 3. Procedures

### 3.1. Schema Changes
- **MUST** be done via EF Core Migrations (Code-First).
- **MUST NOT** modify the database schema manually using SQL scripts unless it's a specific performance index impossible to define in EF.
- **MUST** name migrations descriptively (e.g., `AddUserEmailIndex`, not `Updatedb`).

### 3.2. Data Seeding
- **SHOULD** be idempotent.
- **MUST** check if data exists before inserting.
- **Example**:
  ```csharp
  if (!context.Users.Any()) {
      context.Users.Add(new User { ... });
  }
  ```

### 3.3. Production Deployment
- **MUST** backup the database before applying migrations.
- **SHOULD** generate a SQL script (`dotnet ef migrations script`) to review exactly what will run.
- **MUST** apply migrations inside a transaction.

## 4. Emergency Rollback
- If a migration fails, **IMMEDIATELY** assess data loss risk.
- Use `dotnet ef database update {PreviousMigrationName}` to revert schema.
