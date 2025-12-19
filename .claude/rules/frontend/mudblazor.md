# Reglas de MudBlazor

## Instalación y Configuración

### Program.cs
```csharp
builder.Services.AddMudServices();
```

### _Imports.razor
```razor
@using MudBlazor
```

### App.razor
```razor
<MudThemeProvider />
<MudDialogProvider />
<MudSnackbarProvider />
```

## Componentes Principales

### Layout
```razor
<MudLayout>
    <MudAppBar>
        <MudIconButton Icon="@Icons.Material.Filled.Menu" 
                       OnClick="ToggleDrawer" />
        <MudText Typo="Typo.h6">JSCHUB</MudText>
    </MudAppBar>
    
    <MudDrawer @bind-Open="_drawerOpen" Variant="DrawerVariant.Responsive">
        <MudNavMenu>
            <MudNavLink Href="/" Icon="@Icons.Material.Filled.Home">Inicio</MudNavLink>
        </MudNavMenu>
    </MudDrawer>
    
    <MudMainContent>
        <MudContainer MaxWidth="MaxWidth.Large" Class="mt-4">
            @Body
        </MudContainer>
    </MudMainContent>
</MudLayout>
```

### Tablas
```razor
<MudTable Items="_items" Hover="true" Striped="true" 
          Loading="_loading" LoadingProgressColor="Color.Primary">
    <HeaderContent>
        <MudTh>Nombre</MudTh>
        <MudTh>Acciones</MudTh>
    </HeaderContent>
    <RowTemplate>
        <MudTd>@context.Nombre</MudTd>
        <MudTd>
            <MudIconButton Icon="@Icons.Material.Filled.Edit" 
                           OnClick="@(() => Editar(context))" />
        </MudTd>
    </RowTemplate>
    <PagerContent>
        <MudTablePager />
    </PagerContent>
</MudTable>
```

### Formularios
```razor
<MudForm @ref="_form" @bind-IsValid="_isValid">
    <MudTextField @bind-Value="_model.Nombre" 
                  Label="Nombre" 
                  Required="true"
                  RequiredError="El nombre es requerido" />
    
    <MudSelect @bind-Value="_model.Tipo" Label="Tipo">
        <MudSelectItem Value="@("opcion1")">Opción 1</MudSelectItem>
    </MudSelect>
    
    <MudDatePicker @bind-Date="_model.Fecha" Label="Fecha" />
    
    <MudButton Disabled="!_isValid" OnClick="Guardar">
        Guardar
    </MudButton>
</MudForm>
```

### Diálogos
```csharp
[Inject] private IDialogService DialogService { get; set; }

private async Task MostrarDialogo()
{
    var parameters = new DialogParameters
    {
        ["Item"] = _item
    };
    
    var dialog = await DialogService.ShowAsync<MiDialogo>("Título", parameters);
    var result = await dialog.Result;
    
    if (!result.Canceled)
    {
        // Procesar resultado
    }
}
```

### Notificaciones
```csharp
[Inject] private ISnackbar Snackbar { get; set; }

Snackbar.Add("Guardado exitosamente", Severity.Success);
Snackbar.Add("Error al guardar", Severity.Error);
```

## Theming

```csharp
private MudTheme _theme = new()
{
    PaletteLight = new PaletteLight()
    {
        Primary = Colors.Blue.Default,
        Secondary = Colors.Green.Accent4,
        AppbarBackground = Colors.Blue.Default
    },
    PaletteDark = new PaletteDark()
    {
        Primary = Colors.Blue.Lighten1
    }
};
```

## Iconos
Usar `Icons.Material.Filled.*` o `Icons.Material.Outlined.*`

```razor
<MudIcon Icon="@Icons.Material.Filled.Save" />
```

## Responsive Design

```razor
<MudGrid>
    <MudItem xs="12" sm="6" md="4">
        @* Contenido responsive *@
    </MudItem>
</MudGrid>
```
