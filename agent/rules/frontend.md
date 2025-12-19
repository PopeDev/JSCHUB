# Frontend & UI Guidelines

## Core Principles
1. **MudBlazor First**: Use MudBlazor components for all UI elements.
2. **Responsive Design**: Ensure layouts work on mobile, tablet, and desktop.
3. **User Feedback**: Always provide visual feedback (loading spinners, snackbars) for async operations.

## Component Structure
```razor
@page "/url"
@using Namespaces

<PageTitle>Title</PageTitle>

<MudContainer>
    <!-- Content -->
</MudContainer>

@code {
    // 1. Dependency Injection
    // 2. Parameters
    // 3. Private State
    // 4. Lifecycle Methods
    // 5. Event Handlers
}
```

## State Management
- Use `OnInitializedAsync` for initial data loading.
- Avoid complex logic in getters/setters.
- Use `InvokeAsync(StateHasChanged)` only when necessary and outside the normal lifecycle.

## JavaScript Interop
- Wrap JS calls in specific services or scoped blocks.
- Always implement `IAsyncDisposable` if you keep JS references.
