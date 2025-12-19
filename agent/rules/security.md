# Security Rules

## Vulnerability Prevention

### SQL Injection
- **MUST**: Use Entity Framework Core with LINQ or parametrized queries.
- **MUST NOT**: Concatenate strings to build SQL queries.

### XSS (Cross-Site Scripting)
- **MUST**: Rely on Blazor's automatic HTML encoding for `@variable`.
- **WARNING**: Be extremely careful with `MarkupString`. Only use it with sanitized content.

### CSRF (Cross-Site Request Forgery)
- **MUST**: Use Antiforgery tokens in forms (Blazor standard).

## Authentication & Authorization
- **MUST**: Use `[Authorize]` attribute on pages/components that require login.
- **MUST**: Implement resource-based authorization for strict data access control (e.g., user can only edit their own profile).

## Input Validation
- **MUST**: Validate ALL input at the Application layer using FluentValidation.
- **SHOULD**: Validate UI input using MudBlazor's validation integration.

## Secrets Management
- **MUST NOT**: Commit secrets, keys, or passwords to Git.
- **MUST**: Use User Secrets (`dotnet user-secrets`) for local development.
- **MUST**: Use Environment Variables or Key Vaults in production.
