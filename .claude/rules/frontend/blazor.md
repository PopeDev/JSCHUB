# Reglas de Blazor

## Ciclo de Vida de Componentes

### Orden de Ejecución
1. `SetParametersAsync` - Parámetros recibidos
2. `OnInitialized[Async]` - Inicialización (solo primera vez)
3. `OnParametersSet[Async]` - Parámetros establecidos
4. `OnAfterRender[Async]` - DOM renderizado

### Reglas

```csharp
// ✅ Correcto: Cargar datos en OnInitializedAsync
protected override async Task OnInitializedAsync()
{
    _data = await Service.GetDataAsync();
}

// ❌ Incorrecto: Cargar en constructor
public MiComponente()
{
    _data = Service.GetData(); // No hacer esto
}
```

## Manejo de Estado

### Estado Local
```razor
@code {
    private bool _isVisible = false;
    private List<Item> _items = new();
}
```

### Cascading Values
```razor
@* Padre *@
<CascadingValue Value="_theme">
    <ChildComponent />
</CascadingValue>

@* Hijo *@
[CascadingParameter] private Theme Theme { get; set; }
```

## Renderizado

### InteractiveServer Mode
```razor
@rendermode InteractiveServer
```

### Evitar Re-renders Innecesarios
```csharp
// Implementar ShouldRender si el componente es costoso
protected override bool ShouldRender()
{
    return _hasChanged;
}
```

## JavaScript Interop

```csharp
[Inject] private IJSRuntime JS { get; set; }

// Llamar función JS
await JS.InvokeVoidAsync("functionName", arg1, arg2);

// Con retorno
var result = await JS.InvokeAsync<string>("functionWithReturn");
```

## Dispose Pattern

```csharp
@implements IAsyncDisposable

@code {
    private CancellationTokenSource _cts = new();
    
    public async ValueTask DisposeAsync()
    {
        _cts.Cancel();
        _cts.Dispose();
    }
}
```

## Validación de Formularios

```razor
<EditForm Model="_model" OnValidSubmit="HandleSubmit">
    <FluentValidationValidator />
    <MudTextField @bind-Value="_model.Name" For="@(() => _model.Name)" />
    <MudButton ButtonType="ButtonType.Submit">Guardar</MudButton>
</EditForm>
```
