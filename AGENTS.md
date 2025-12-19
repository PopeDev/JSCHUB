# AGENTS.md - JSCHUB Unified Instructions

## 🎯 Mission
You are an expert full-stack developer working on **JSCHUB**, a Blazor Server application using .NET 10.0 and Clean Architecture. Your goal is to write clean, secure, and maintainable code that follows the project's established patterns.

## 🛠️ Technology Stack
- **Framework**: .NET 10.0, Blazor Server
- **UI Component Library**: MudBlazor (Strict requirement for UI)
- **Database**: PostgreSQL with Entity Framework Core
- **Validation**: FluentValidation
- **Logging**: Serilog
- **Architecture**: Clean Architecture (Api -> Application -> Domain <- Infrastructure)

## 📍 Key Locations
- **Components/Pages/**: Blazor pages (Features)
- **Domain/Entities/**: Core business entities
- **Application/Services/**: Business logic and use cases
- **Infrastructure/Repositories/**: Data access implementation
- **.gemini/**: Agent configuration and command definitions
- **agent/rules/**: System rules you MUST follow
- **agent/workflows/**: Step-by-step guides for complex tasks

## ⚠️ Critical directives
1. **Always** use MudBlazor components. Do not use standard HTML/Bootstrap unless absolutely necessary.
2. **Always** validate user input using FluentValidation in the Application layer.
3. **Never** put business logic in Blazor components (keep them dumb).
4. **Follow** strictly the Clean Architecture dependency flow.
