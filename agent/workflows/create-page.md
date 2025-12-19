---
description: Create a new Blazor page with MudBlazor
---

# Create Page Workflow

1. **Define Requirements**
   - Identify the URL route (e.g., `/products`).
   - Identify required services (e.g., `IProductService`).

2. **Create Razor Component**
   - File: `Components/Pages/{PageName}.razor`
   - Add `@page "/{route}"`
   - Add `@using` directives

3. **Implement UI Layout**
   - Use `MudContainer`, `MudGrid`, `MudPaper`.
   - Add `MudText` for titles.

4. **Implement Logic**
   - Inject services.
   - Implement `OnInitializedAsync` to load data.
   - Add error handling (try/catch + Snackbar).

5. **Add Navigation**
   - Add `MudNavLink` to `Components/Layout/NavMenu.razor`.
