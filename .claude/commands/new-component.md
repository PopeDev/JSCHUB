# ➕ Comando: Nuevo Componente Blazor

## Descripción
Crea un nuevo componente Blazor siguiendo las convenciones del proyecto.

## Pasos

### 1. Crear Archivo
Ubicación: `Components/Pages/` o `Components/Shared/`

Nombre: `NombreComponente.razor`

### 2. Estructura Base

```razor
@page "/ruta" @* Solo si es página *@

<PageTitle>Título de la Página</PageTitle>

<MudContainer MaxWidth="MaxWidth.Large" Class="mt-4">
    <MudText Typo="Typo.h4" Class="mb-4">Título</MudText>
    
    @* Contenido del componente *@
    
</MudContainer>

@code {
    // Inyección de dependencias
    [Inject] private IServicio Servicio { get; set; } = default!;
    
    // Parámetros
    [Parameter] public string? Parametro { get; set; }
    
    // Estado
    private bool _loading = true;
    private List<Modelo> _items = new();
    
    // Lifecycle
    protected override async Task OnInitializedAsync()
    {
        await CargarDatos();
    }
    
    // Métodos
    private async Task CargarDatos()
    {
        _loading = true;
        try
        {
            _items = await Servicio.GetAllAsync();
        }
        finally
        {
            _loading = false;
        }
    }
}
```

### 3. Convenciones

| Elemento | Convención |
|----------|------------|
| Nombre archivo | PascalCase.razor |
| Variables privadas | `_camelCase` |
| Parámetros | `[Parameter]` PascalCase |
| Servicios | `[Inject]` interfaz |

### 4. Componentes MudBlazor Comunes

```razor
@* Loading *@
<MudProgressLinear Indeterminate="true" Visible="_loading" />

@* Tabla *@
<MudTable Items="_items" Hover="true" Striped="true">

@* Formulario *@
<MudForm @ref="_form" Model="_model">

@* Botón *@
<MudButton Variant="Variant.Filled" Color="Color.Primary">
```
