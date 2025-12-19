# SOP: Crear Nueva Página Blazor

## Objetivo
Procedimiento para crear nuevas páginas en la aplicación Blazor con MudBlazor.

## Pasos

### 1. Crear Archivo de Página

Ubicación: `Components/Pages/NuevaPagina.razor`

### 2. Estructura Base

```razor
@page "/nueva-pagina"
@rendermode InteractiveServer

<PageTitle>Nueva Página - JSCHUB</PageTitle>

<MudContainer MaxWidth="MaxWidth.Large" Class="mt-4">
    <MudText Typo="Typo.h4" Class="mb-4">
        <MudIcon Icon="@Icons.Material.Filled.Category" Class="mr-2" />
        Nueva Página
    </MudText>

    @if (_loading)
    {
        <MudProgressLinear Indeterminate="true" Color="Color.Primary" />
    }
    else
    {
        <MudPaper Class="pa-4">
            @* Contenido de la página *@
        </MudPaper>
    }
</MudContainer>

@code {
    [Inject] private IServicioRequerido Servicio { get; set; } = default!;
    [Inject] private ISnackbar Snackbar { get; set; } = default!;
    
    private bool _loading = true;
    private List<ModeloDto> _items = new();
    
    protected override async Task OnInitializedAsync()
    {
        await CargarDatosAsync();
    }
    
    private async Task CargarDatosAsync()
    {
        _loading = true;
        try
        {
            _items = await Servicio.GetAllAsync();
        }
        catch (Exception ex)
        {
            Snackbar.Add($"Error: {ex.Message}", Severity.Error);
        }
        finally
        {
            _loading = false;
        }
    }
}
```

### 3. Página con Formulario

```razor
@page "/nueva-pagina/crear"

<MudContainer MaxWidth="MaxWidth.Medium" Class="mt-4">
    <MudText Typo="Typo.h5" Class="mb-4">Crear Nuevo Elemento</MudText>
    
    <MudPaper Class="pa-4">
        <MudForm @ref="_form" Model="_model" @bind-IsValid="_isValid">
            <MudGrid>
                <MudItem xs="12">
                    <MudTextField @bind-Value="_model.Nombre" 
                                  Label="Nombre" 
                                  Required="true" />
                </MudItem>
                <MudItem xs="12" Class="d-flex justify-end gap-2">
                    <MudButton OnClick="Cancelar">Cancelar</MudButton>
                    <MudButton Variant="Variant.Filled" 
                               Color="Color.Primary"
                               Disabled="!_isValid"
                               OnClick="GuardarAsync">
                        Guardar
                    </MudButton>
                </MudItem>
            </MudGrid>
        </MudForm>
    </MudPaper>
</MudContainer>

@code {
    [Inject] private NavigationManager Navigation { get; set; } = default!;
    
    private MudForm _form = default!;
    private bool _isValid;
    private CreateModeloDto _model = new();
    
    private void Cancelar() => Navigation.NavigateTo("/nueva-pagina");
    
    private async Task GuardarAsync()
    {
        await _form.Validate();
        if (_isValid)
        {
            await Servicio.CreateAsync(_model);
            Snackbar.Add("Guardado exitosamente", Severity.Success);
            Navigation.NavigateTo("/nueva-pagina");
        }
    }
}
```

### 4. Agregar al Menú de Navegación

```razor
<!-- En NavMenu.razor -->
<MudNavLink Href="/nueva-pagina" 
            Icon="@Icons.Material.Filled.Category">
    Nueva Página
</MudNavLink>
```

## Checklist

```markdown
- [ ] Archivo .razor creado
- [ ] @page definido con ruta
- [ ] @rendermode InteractiveServer añadido
- [ ] PageTitle configurado
- [ ] Servicios inyectados
- [ ] Estado de loading implementado
- [ ] Manejo de errores con Snackbar
- [ ] Navegación añadida al menú
```
