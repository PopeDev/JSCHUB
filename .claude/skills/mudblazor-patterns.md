# Patrones MudBlazor

## Plantillas Reutilizables

### Tabla con CRUD Completo

```razor
<MudTable Items="_items" Hover="true" Striped="true" 
          Loading="_loading" LoadingProgressColor="Color.Primary"
          Filter="@(new Func<ItemDto, bool>(FilterFunc))">
    <ToolBarContent>
        <MudText Typo="Typo.h6">Listado de Items</MudText>
        <MudSpacer />
        <MudTextField @bind-Value="_searchString" 
                      Placeholder="Buscar..." 
                      Adornment="Adornment.Start" 
                      AdornmentIcon="@Icons.Material.Filled.Search"
                      IconSize="Size.Medium" 
                      Class="mt-0" />
        <MudButton Variant="Variant.Filled" 
                   Color="Color.Primary" 
                   StartIcon="@Icons.Material.Filled.Add"
                   OnClick="AbrirCrear"
                   Class="ml-4">
            Nuevo
        </MudButton>
    </ToolBarContent>
    <HeaderContent>
        <MudTh><MudTableSortLabel SortBy="new Func<ItemDto, object>(x => x.Nombre)">Nombre</MudTableSortLabel></MudTh>
        <MudTh>Estado</MudTh>
        <MudTh Style="width: 150px">Acciones</MudTh>
    </HeaderContent>
    <RowTemplate>
        <MudTd>@context.Nombre</MudTd>
        <MudTd>
            <MudChip Color="@(context.Activo ? Color.Success : Color.Default)" Size="Size.Small">
                @(context.Activo ? "Activo" : "Inactivo")
            </MudChip>
        </MudTd>
        <MudTd>
            <MudIconButton Icon="@Icons.Material.Filled.Edit" 
                           Size="Size.Small"
                           OnClick="@(() => Editar(context))" />
            <MudIconButton Icon="@Icons.Material.Filled.Delete" 
                           Size="Size.Small" 
                           Color="Color.Error"
                           OnClick="@(() => Eliminar(context))" />
        </MudTd>
    </RowTemplate>
    <PagerContent>
        <MudTablePager PageSizeOptions="new int[] { 10, 25, 50 }" />
    </PagerContent>
</MudTable>

@code {
    private string _searchString = "";
    
    private bool FilterFunc(ItemDto item) =>
        string.IsNullOrWhiteSpace(_searchString) ||
        item.Nombre.Contains(_searchString, StringComparison.OrdinalIgnoreCase);
}
```

### Formulario con Validación

```razor
<MudDialog>
    <TitleContent>
        <MudText Typo="Typo.h6">
            @(_isEditing ? "Editar" : "Crear") Item
        </MudText>
    </TitleContent>
    <DialogContent>
        <MudForm @ref="_form" Model="_model" @bind-IsValid="_isValid">
            <MudGrid>
                <MudItem xs="12">
                    <MudTextField @bind-Value="_model.Nombre"
                                  Label="Nombre"
                                  Required="true"
                                  RequiredError="El nombre es requerido"
                                  Variant="Variant.Outlined" />
                </MudItem>
                <MudItem xs="12" sm="6">
                    <MudSelect @bind-Value="_model.TipoId" 
                               Label="Tipo"
                               Variant="Variant.Outlined">
                        @foreach (var tipo in _tipos)
                        {
                            <MudSelectItem Value="@tipo.Id">@tipo.Nombre</MudSelectItem>
                        }
                    </MudSelect>
                </MudItem>
                <MudItem xs="12" sm="6">
                    <MudDatePicker @bind-Date="_model.Fecha" 
                                   Label="Fecha"
                                   Variant="Variant.Outlined" />
                </MudItem>
                <MudItem xs="12">
                    <MudSwitch @bind-Value="_model.Activo" 
                               Label="Activo" 
                               Color="Color.Primary" />
                </MudItem>
            </MudGrid>
        </MudForm>
    </DialogContent>
    <DialogActions>
        <MudButton OnClick="Cancel">Cancelar</MudButton>
        <MudButton Variant="Variant.Filled" 
                   Color="Color.Primary" 
                   Disabled="!_isValid || _saving"
                   OnClick="Submit">
            @if (_saving)
            {
                <MudProgressCircular Size="Size.Small" Indeterminate="true" Class="mr-2" />
            }
            Guardar
        </MudButton>
    </DialogActions>
</MudDialog>
```

### Confirmación de Eliminación

```razor
<MudDialog>
    <DialogContent>
        <MudAlert Severity="Severity.Warning" Class="mb-4">
            Esta acción no se puede deshacer.
        </MudAlert>
        <MudText>¿Está seguro que desea eliminar "@NombreItem"?</MudText>
    </DialogContent>
    <DialogActions>
        <MudButton OnClick="Cancel">Cancelar</MudButton>
        <MudButton Variant="Variant.Filled" 
                   Color="Color.Error" 
                   OnClick="Confirm">
            Eliminar
        </MudButton>
    </DialogActions>
</MudDialog>

@code {
    [CascadingParameter] private MudDialogInstance MudDialog { get; set; } = default!;
    [Parameter] public string NombreItem { get; set; } = "";
    
    private void Cancel() => MudDialog.Cancel();
    private void Confirm() => MudDialog.Close(DialogResult.Ok(true));
}
```

### Card de Estadísticas

```razor
<MudCard Class="stat-card">
    <MudCardContent Class="d-flex align-center">
        <MudAvatar Color="@Color" Size="Size.Large" Class="mr-4">
            <MudIcon Icon="@Icon" />
        </MudAvatar>
        <div>
            <MudText Typo="Typo.h4">@Value</MudText>
            <MudText Typo="Typo.body2" Class="mud-text-secondary">@Label</MudText>
        </div>
    </MudCardContent>
</MudCard>

@code {
    [Parameter] public string Icon { get; set; } = Icons.Material.Filled.Info;
    [Parameter] public Color Color { get; set; } = Color.Primary;
    [Parameter] public string Value { get; set; } = "0";
    [Parameter] public string Label { get; set; } = "Label";
}
```

### Stepper para Procesos

```razor
<MudStepper @bind-ActiveStepIndex="_currentStep" Linear="true">
    <MudStep Title="Información Básica">
        <MudTextField @bind-Value="_model.Nombre" Label="Nombre" Required="true" />
    </MudStep>
    <MudStep Title="Detalles">
        <MudTextField @bind-Value="_model.Descripcion" Label="Descripción" Lines="3" />
    </MudStep>
    <MudStep Title="Confirmación">
        <MudText>Revise los datos antes de guardar.</MudText>
        <MudList>
            <MudListItem>Nombre: @_model.Nombre</MudListItem>
            <MudListItem>Descripción: @_model.Descripcion</MudListItem>
        </MudList>
    </MudStep>
</MudStepper>
```

### Loading Skeleton

```razor
@if (_loading)
{
    <MudSkeleton SkeletonType="SkeletonType.Rectangle" Height="200px" Class="mb-4" />
    <MudSkeleton SkeletonType="SkeletonType.Text" Class="mb-2" />
    <MudSkeleton SkeletonType="SkeletonType.Text" Width="60%" />
}
else
{
    @* Contenido real *@
}
```
