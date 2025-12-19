---
description: Manage Entity Framework Core migrations
---

# Database Migration Workflow

1. **Check Pending Changes**
   - Ensure your Entity classes and DbContext configurations reflect the desired state.

2. **Add Migration**
   - Prompt the user for a descriptive migration name.
   - Run: `dotnet ef migrations add {MigrationName}`

3. **Review Migration**
   - Open the generated file in `Infrastructure/Data/Migrations/`.
   - Check `Up()` and `Down()` methods for accuracy.
   - CAUTION: Check for destructive operations (DropTable, DropColumn).

4. **Update Database (Local)**
   // turbo
   - Run: `dotnet ef database update`

5. **Verify**
   - Check the database schema to ensure changes were applied correctly.
