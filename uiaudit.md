# Auditoría de UI - JSCHUB Blazor Server

**Fecha:** 2025-12-21
**Revisor:** Senior UI Reviewer
**Stack:** .NET 10, Blazor Server, MudBlazor
**Versión de la Aplicación:** POC/MVP con autenticación hardcodeada

---

## 1. Mapa de UI y Flujos

### 1.1 Estructura de Rutas

| Ruta | Componente | Auth Required | Descripción |
|------|-----------|---------------|-------------|
| `/` | `Home.razor` | No | Landing page con accesos rápidos |
| `/login` | `Login.razor` | No | Formulario de login |
| `/counter` | `Counter.razor` | No | Ejemplo Blazor |
| `/weather` | `Weather.razor` | No | Ejemplo Blazor |
| `/Error` | `Error.razor` | No | Página de error |
| `/not-found` | `NotFound.razor` | No | Página 404 |
| `/backoffice` | `Dashboard.razor` | **Sí** | Dashboard principal |
| `/backoffice/dashboard` | `Dashboard.razor` | **Sí** | Dashboard (alias) |
| `/backoffice/monitor` | `Monitor.razor` | **Sí** | Panel de alertas |
| `/backoffice/reminders` | `Reminders.razor` | **Sí** | Listado recordatorios |
| `/backoffice/reminders/new` | `ReminderForm.razor` | **Sí** | Crear recordatorio |
| `/backoffice/reminders/{Id}/edit` | `ReminderForm.razor` | **Sí** | Editar recordatorio |
| `/backoffice/reminders/{Id}` | `ReminderDetail.razor` | **Sí** | Detalle recordatorio |
| `/backoffice/gastos` | `Gastos.razor` | **Sí** | Listado gastos |
| `/backoffice/gastos/new` | `GastoForm.razor` | **Sí** | Crear gasto |
| `/backoffice/gastos/{Id}/edit` | `GastoForm.razor` | **Sí** | Editar gasto |
| `/backoffice/gastos/{Id}` | `GastoDetail.razor` | **Sí** | Detalle gasto |
| `/backoffice/proyectos` | `Proyectos.razor` | **Sí** | Listado proyectos |
| `/backoffice/proyectos/new` | `ProyectoForm.razor` | **Sí** | Crear proyecto |
| `/backoffice/proyectos/{Id}/edit` | `ProyectoForm.razor` | **Sí** | Editar proyecto |
| `/backoffice/proyectos/{Id}` | `ProyectoDetail.razor` | **Sí** | Detalle proyecto |
| `/backoffice/usuarios` | `Usuarios.razor` | **Sí** | Listado usuarios |
| `/backoffice/usuarios/new` | `UsuarioForm.razor` | **Sí** | Crear usuario |
| `/backoffice/usuarios/{Id}/edit` | `UsuarioForm.razor` | **Sí** | Editar usuario |

### 1.2 Flujo de Autenticación

```
┌─────────────┐     ┌─────────────┐     ┌─────────────────┐
│   Home (/)  │────>│   Login     │────>│   /backoffice   │
└─────────────┘     │  (/login)   │     │    /monitor     │
                    └─────────────┘     └─────────────────┘
                          │                      │
                          │                      │ Logout
                          │<─────────────────────┘
                          │
                    [forceLoad: true]
```

### 1.3 Componentes Principales

- **MainLayout.razor**: Layout con MudAppBar, MudDrawer, control de auth
- **Login.razor**: Formulario de autenticación (credenciales POC)
- **AuthService.cs**: Servicio de autenticación POC (Singleton)

### 1.4 Credenciales POC (No modificar)

| Usuario | Contraseña |
|---------|------------|
| Pope | 00000 |
| Javi | 123456 |
| Carlos | 010203 |

---

## 2. Hallazgos Priorizados

### 2.1 [CRÍTICO] AuthService Singleton Compartido Entre Usuarios

| Campo | Valor |
|-------|-------|
| **Severidad** | Crítico |
| **Tipo** | Auth-UI / Concurrencia |
| **Dónde** | `Program.cs:42-44` + `Infrastructure/Services/AuthService.cs` |
| **Síntoma visible** | Si el Usuario A hace login, el Usuario B (en otro navegador/dispositivo) ve la sesión de A. Si A hace logout, B también pierde la sesión. |
| **Causa probable** | `AuthService` está registrado como `Singleton`. En Blazor Server, múltiples usuarios comparten la misma instancia del servicio. El campo `_currentUser` es compartido entre TODOS los circuitos de todos los usuarios. |

**Cómo reproducir:**
1. Abrir la app en el navegador A, hacer login como "Pope"
2. Abrir la app en el navegador B (incógnito o distinto dispositivo)
3. El navegador B verá que está logueado como "Pope" sin haber introducido credenciales
4. Hacer logout en navegador A
5. Navegador B pierde la sesión inmediatamente

**Fix recomendado (respetando POC):**
Cambiar `AuthService` de `Singleton` a `Scoped`, pero almacenar el estado de sesión en el circuito/conexión. Para POC, la solución mínima:

```csharp
// Program.cs - Cambiar de Singleton a Scoped
builder.Services.AddScoped<JSCHUB.Infrastructure.Services.AuthService>();
builder.Services.AddScoped<IAuthService>(sp => sp.GetRequiredService<JSCHUB.Infrastructure.Services.AuthService>());
builder.Services.AddScoped<ICurrentUserService>(sp => sp.GetRequiredService<JSCHUB.Infrastructure.Services.AuthService>());
```

Esto mantiene el comportamiento POC hardcodeado pero cada circuito/usuario tiene su propia instancia de sesión.

---

### 2.2 [ALTO] Login.razor - NavigateTo en OnInitialized Síncrono

| Campo | Valor |
|-------|-------|
| **Severidad** | Alto |
| **Tipo** | Navegación / Render |
| **Dónde** | `Components/Pages/Login.razor:77-84` - método `OnInitialized()` |
| **Síntoma visible** | Posible flickering o doble render cuando el usuario ya está autenticado y accede a `/login`. En algunos casos, la navegación puede no ejecutarse correctamente. |
| **Causa probable** | Se ejecuta `Navigation.NavigateTo()` durante el ciclo de vida síncrono `OnInitialized()`. En Blazor Server, las navegaciones durante la inicialización síncrona pueden causar comportamientos inesperados. |

**Cómo reproducir:**
1. Hacer login correctamente
2. Navegar manualmente a `/login` escribiendo la URL
3. Observar un posible flash de la pantalla de login antes de redirigir

**Fix recomendado:**
```csharp
protected override async Task OnInitializedAsync()
{
    if (CurrentUser.IsAuthenticated)
    {
        await Task.Yield(); // Permitir que el render inicial complete
        Navigation.NavigateTo("/backoffice/monitor");
    }
}
```

---

### 2.3 [ALTO] MainLayout - Redirección en Evento Sin InvokeAsync

| Campo | Valor |
|-------|-------|
| **Severidad** | Alto |
| **Tipo** | Navegación / Render |
| **Dónde** | `Components/Layout/MainLayout.razor:77-81` - método `OnLocationChanged()` |
| **Síntoma visible** | Posibles race conditions donde la redirección a `/login` no ocurre inmediatamente o el estado de UI queda inconsistente tras cambio de ruta. |
| **Causa probable** | `OnLocationChanged` es un event handler que se ejecuta fuera del contexto de render de Blazor. Llamar a `CheckAuthentication()` (que navega) sin `InvokeAsync` puede causar problemas de sincronización. |

**Cómo reproducir:**
1. Estar logueado en `/backoffice/monitor`
2. Hacer logout en otra pestaña o modificar el estado de auth manualmente
3. Navegar a otra ruta de backoffice
4. Observar comportamiento inconsistente (a veces redirige, a veces muestra contenido parcial)

**Fix recomendado:**
```csharp
private async void OnLocationChanged(object? sender, LocationChangedEventArgs e)
{
    await InvokeAsync(() =>
    {
        CheckAuthentication();
        StateHasChanged();
    });
}
```

---

### 2.4 [ALTO] Pérdida de Sesión en Reconexión de Circuito

| Campo | Valor |
|-------|-------|
| **Severidad** | Alto |
| **Tipo** | Auth-UI / Blazor Server |
| **Dónde** | Todo el flujo de autenticación |
| **Síntoma visible** | Si la conexión SignalR se pierde y reconecta (común en móviles o conexiones inestables), el estado de autenticación puede perderse o comportarse de forma impredecible. |
| **Causa probable** | El estado de `_currentUser` en `AuthService` está en memoria del servidor. Si el circuito se reconecta a una nueva instancia (o si el servidor reinicia), la sesión se pierde. No hay `ReconnectModal` que maneje esto correctamente. |

**Cómo reproducir:**
1. Hacer login
2. Desconectar internet brevemente (simular con DevTools Network offline)
3. Reconectar
4. Observar que el modal de reconexión aparece pero el estado de auth puede estar corrupto

**Fix recomendado (sin cambiar POC):**
Mejorar `ReconnectModal.razor` para incluir lógica que fuerce un refresh completo de la página tras reconexión, o mostrar mensaje claro de "Sesión expirada, por favor recarga la página".

---

### 2.5 [MEDIO] Monitor.razor - Acciones Sin Estado de Loading Individual

| Campo | Valor |
|-------|-------|
| **Severidad** | Medio |
| **Tipo** | Interacción / UX |
| **Dónde** | `Components/Pages/Backoffice/Monitor.razor:127-139` |
| **Síntoma visible** | El usuario puede hacer doble-click en "Reconocer", "Posponer" o "Completar" y ejecutar la acción múltiples veces mientras la primera request está en curso. |
| **Causa probable** | Los botones de acción no tienen estado `Disabled` individual durante la ejecución de la acción async. |

**Cómo reproducir:**
1. Ir al Monitor de Alertas
2. Hacer doble-click rápido en "Reconocer" en una alerta
3. Observar que se hace la petición dos veces (verificable en logs del servidor)

**Fix recomendado:**
```csharp
@code {
    private HashSet<Guid> _processingAlerts = [];

    private async Task AcknowledgeAsync(Guid alertId)
    {
        if (_processingAlerts.Contains(alertId)) return;
        _processingAlerts.Add(alertId);
        try
        {
            await AlertService.AcknowledgeAsync(alertId);
            Snackbar.Add("Alerta reconocida", MudBlazor.Severity.Info);
            await LoadDataAsync();
        }
        finally
        {
            _processingAlerts.Remove(alertId);
        }
    }
}

// En el template:
<MudButton Disabled="@(_processingAlerts.Contains(alert.Id))" ...>
```

---

### 2.6 [MEDIO] Usuarios.razor - Mensaje de Toggle Incorrecto

| Campo | Valor |
|-------|-------|
| **Severidad** | Medio |
| **Tipo** | UX / Lógica |
| **Dónde** | `Components/Pages/Backoffice/Usuarios.razor:139-145` - método `ToggleActivoAsync()` |
| **Síntoma visible** | El mensaje de Snackbar dice "Usuario desactivado" cuando el usuario estaba activo, pero el servidor ya cambió el estado, así que el mensaje es correcto en intención pero basado en el estado antiguo que ya no es válido. |
| **Causa probable** | Se lee `usuario.Activo` después de `ToggleActivoAsync()`, pero `usuario` es una referencia al objeto local que aún no se ha actualizado (la lista `_usuarios` se recarga después). |

**Cómo reproducir:**
1. Ir a Usuarios
2. Click en "Desactivar" para un usuario activo
3. Mensaje correcto aparece ("desactivado")
4. El problema está en que si la petición falla, el mensaje sería incorrecto

**Fix recomendado:**
```csharp
private async Task ToggleActivoAsync(Guid id)
{
    var usuario = _usuarios.First(u => u.Id == id);
    var nuevoEstado = usuario.Activo ? "desactivado" : "activado";
    await UsuarioService.ToggleActivoAsync(id);
    Snackbar.Add($"Usuario {nuevoEstado}", MudBlazor.Severity.Success);
    await LoadDataAsync();
}
```

---

### 2.7 [MEDIO] Reminders.razor - Filtros No Disparan Búsqueda Automática

| Campo | Valor |
|-------|-------|
| **Severidad** | Medio |
| **Tipo** | UX / Interacción |
| **Dónde** | `Components/Pages/Backoffice/Reminders.razor:31-46` |
| **Síntoma visible** | Cambiar la Categoría o Estado en los selectores no actualiza la tabla automáticamente. El usuario debe hacer click en "Filtrar". |
| **Causa probable** | Los `MudSelect` usan `@bind-Value` pero no tienen `ValueChanged` handler para disparar la búsqueda. Solo el campo de texto tiene `OnDebounceIntervalElapsed`. |

**Cómo reproducir:**
1. Ir a Recordatorios
2. Cambiar el filtro de "Categoría" a "Renovación"
3. Observar que la tabla no cambia
4. Hacer click en "Filtrar" para ver los resultados

**Fix recomendado:**
```razor
<MudSelect T="Category?" @bind-Value="_filterCategory" Label="Categoría"
           Clearable="true" Variant="Variant.Outlined" Dense="true"
           ValueChanged="@(async (Category? c) => { _filterCategory = c; await LoadDataAsync(); })">
```

---

### 2.8 [MEDIO] ReminderForm.razor - MudDatePicker Sin Validación Required

| Campo | Valor |
|-------|-------|
| **Severidad** | Medio |
| **Tipo** | Validación |
| **Dónde** | `Components/Pages/Backoffice/ReminderForm.razor:61-62` |
| **Síntoma visible** | El usuario puede borrar la fecha de vencimiento y guardar el recordatorio sin fecha, lo cual puede causar comportamientos inesperados en el sistema de alertas. |
| **Causa probable** | `MudDatePicker` no tiene `Required="true"` ni `RequiredError`. |

**Cómo reproducir:**
1. Crear nuevo recordatorio
2. Borrar la fecha del DatePicker (si es posible) o dejarla sin seleccionar
3. Guardar
4. El recordatorio se crea sin fecha de próxima ocurrencia

**Fix recomendado:**
```razor
<MudDatePicker @bind-Date="_dueAt"
               Label="@(_scheduleType == ScheduleType.OneTime ? "Fecha de vencimiento" : "Primera ocurrencia")"
               Variant="Variant.Outlined"
               Required="true"
               RequiredError="La fecha es obligatoria" />
```

---

### 2.9 [MEDIO] ProyectoForm.razor - Usuario Hardcodeado en Lugar de CurrentUserService

| Campo | Valor |
|-------|-------|
| **Severidad** | Medio |
| **Tipo** | Lógica / Auditoría |
| **Dónde** | `Components/Pages/Backoffice/ProyectoForm.razor:192, 205` |
| **Síntoma visible** | Los campos `CreadoPor` y `ModificadoPor` siempre muestran "usuario" en lugar del nombre del usuario logueado. |
| **Causa probable** | Se pasa la cadena `"usuario"` hardcodeada en lugar de usar `ICurrentUserService.CurrentUserName`. Hay un comentario `// TODO: obtener usuario actual`. |

**Cómo reproducir:**
1. Login como "Pope"
2. Crear un nuevo proyecto
3. Ver el detalle del proyecto
4. Observar que "Creado por: usuario" en lugar de "Creado por: Pope"

**Fix recomendado:**
```csharp
@inject ICurrentUserService CurrentUserService

// En SaveAsync:
await ProyectoService.CreateAsync(dto, CurrentUserService.CurrentUserName ?? "anónimo");
```

---

### 2.10 [MEDIO] GastoForm.razor - Validación Manual Fuera del Flujo de MudForm

| Campo | Valor |
|-------|-------|
| **Severidad** | Medio |
| **Tipo** | Validación / UX |
| **Dónde** | `Components/Pages/Backoffice/GastoForm.razor:160-164` |
| **Síntoma visible** | La validación de importe > 0 se hace después de `_form.Validate()`, mostrando un Snackbar de error en lugar de un mensaje de validación inline. Experiencia inconsistente. |
| **Causa probable** | Se agregó validación manual con Snackbar en lugar de usar la validación integrada de MudNumericField. |

**Cómo reproducir:**
1. Crear nuevo gasto
2. Dejar importe en 0 o vacío
3. Hacer click en Guardar
4. Ver Snackbar rojo en lugar de mensaje de error bajo el campo

**Fix recomendado:**
```razor
<MudNumericField @bind-Value="_importe" Label="Importe" Required="true"
                 Min="0.01m" Format="N2"
                 Validation="@(new Func<decimal, string?>(v => v <= 0 ? "El importe debe ser mayor que 0" : null))"
                 Adornment="Adornment.End" AdornmentText="@_moneda" />
```

---

### 2.11 [BAJO] Gastos.razor - Parámetro No Usado en Lambda

| Campo | Valor |
|-------|-------|
| **Severidad** | Bajo |
| **Tipo** | Código / Mantenibilidad |
| **Dónde** | `Components/Pages/Backoffice/Gastos.razor:29` |
| **Síntoma visible** | Ninguno visible, pero el parámetro `string s` no se usa. |
| **Causa probable** | Copia/pega o desconocimiento de la firma del evento. |

**Fix recomendado:**
```razor
OnDebounceIntervalElapsed="LoadDataAsync"
```
O si se requiere mantener la firma async:
```razor
OnDebounceIntervalElapsed="@(async () => await LoadDataAsync())"
```

---

### 2.12 [BAJO] Home.razor - Enlaces a Backoffice Sin Verificar Auth

| Campo | Valor |
|-------|-------|
| **Severidad** | Bajo |
| **Tipo** | UX / Navegación |
| **Dónde** | `Components/Pages/Home.razor:31, 55, 79` |
| **Síntoma visible** | Un usuario no autenticado puede hacer click en "Ir al Monitor" y será redirigido a `/login`, pero la experiencia podría ser más clara. |
| **Causa probable** | Los enlaces van directamente a rutas protegidas sin verificar el estado de auth previamente. |

**Fix recomendado (opcional):**
Mostrar los enlaces condicionalmente o con indicación visual de que requieren login.

---

### 2.13 [BAJO] Monitor.razor - MudDialog Sin Configuración de Backdrop

| Campo | Valor |
|-------|-------|
| **Severidad** | Bajo |
| **Tipo** | UX |
| **Dónde** | `Components/Pages/Backoffice/Monitor.razor:152-164` |
| **Síntoma visible** | El diálogo de "Posponer" puede cerrarse haciendo click fuera de él o presionando Escape, lo cual puede causar confusión si el usuario lo cierra accidentalmente. |
| **Causa probable** | No se han configurado las opciones del diálogo. |

**Fix recomendado:**
```razor
<MudDialog @bind-Visible="_snoozeDialogVisible" Options="new DialogOptions { CloseOnEscapeKey = false, BackdropClick = false }">
```

---

### 2.14 [BAJO] Proyectos.razor - Falta Sincronización Filtro Estado con Incluir Archivados

| Campo | Valor |
|-------|-------|
| **Severidad** | Bajo |
| **Tipo** | UX / Lógica |
| **Dónde** | `Components/Pages/Backoffice/Proyectos.razor:35-49` |
| **Síntoma visible** | Se puede seleccionar "Estado: Archivado" pero tener "Incluir archivados" desactivado, lo cual puede dar resultados vacíos confusos. |
| **Causa probable** | Los dos controles de filtro no tienen lógica de sincronización. |

**Fix recomendado:**
Si se selecciona "Archivado" en el filtro de estado, activar automáticamente "Incluir archivados", o deshabilitar la opción de filtrar por "Archivado" si el switch está desactivado.

---

### 2.15 [SOSPECHA FUNDADA] Posible Race Condition en LoadDataAsync Concurrent

| Campo | Valor |
|-------|-------|
| **Severidad** | Medio (sospecha) |
| **Tipo** | Concurrencia |
| **Dónde** | Múltiples componentes: `Monitor.razor`, `Dashboard.razor`, `Reminders.razor`, `Gastos.razor` |
| **Síntoma visible** | Si el usuario hace click rápidamente en "Actualizar" o cambia filtros mientras una request está en curso, pueden pisarse los resultados mostrando datos obsoletos. |
| **Causa probable** | No hay `CancellationToken` para cancelar requests anteriores cuando se inicia una nueva. |

**Escenario hipotético:**
1. Usuario aplica filtro A → Request A inicia (3 segundos)
2. Usuario aplica filtro B (antes de que A termine) → Request B inicia (1 segundo)
3. Request B completa → UI muestra resultados de B
4. Request A completa → UI muestra resultados de A (incorrectos para el filtro actual)

**Fix recomendado:**
Implementar patrón con `CancellationTokenSource`:
```csharp
private CancellationTokenSource? _cts;

private async Task LoadDataAsync()
{
    _cts?.Cancel();
    _cts = new CancellationTokenSource();
    var token = _cts.Token;

    _loading = true;
    try
    {
        var result = await SomeService.SearchAsync(..., token);
        if (!token.IsCancellationRequested)
        {
            _items = result.ToList();
        }
    }
    catch (OperationCanceledException) { }
    finally
    {
        if (!token.IsCancellationRequested)
            _loading = false;
    }
}
```

---

## 3. Checklist de Regresión UI

### 3.1 Flujo de Autenticación

| # | Caso de Prueba | Resultado Esperado |
|---|---------------|-------------------|
| 1 | Acceder a `/login` sin autenticar | Ver formulario de login |
| 2 | Login con credenciales correctas (Pope/00000) | Redirección a `/backoffice/monitor` |
| 3 | Login con credenciales incorrectas | Mensaje de error visible |
| 4 | Login con campos vacíos | Validación bloquea envío |
| 5 | Acceder a `/backoffice/*` sin autenticar | Redirección a `/login` |
| 6 | Logout desde cualquier página de backoffice | Redirección a `/login` |
| 7 | Acceder a `/login` estando autenticado | Redirección a `/backoffice/monitor` |
| 8 | Refrescar página (F5) en backoffice estando logueado | Mantener sesión (verificar tras fix Singleton) |

### 3.2 Formularios CRUD

| # | Caso de Prueba | Resultado Esperado |
|---|---------------|-------------------|
| 9 | Crear recordatorio con todos los campos | Guardado exitoso, snackbar verde |
| 10 | Editar recordatorio existente | Datos precargados, guardado actualiza |
| 11 | Crear gasto sin importe | Validación bloquea o muestra error |
| 12 | Crear proyecto con URL inválida | Mensaje de validación inline |
| 13 | Doble-click en "Guardar" en formulario | Solo una petición ejecutada |
| 14 | Cancelar formulario en edición | Volver a listado sin cambios |

### 3.3 Listados y Filtros

| # | Caso de Prueba | Resultado Esperado |
|---|---------------|-------------------|
| 15 | Buscar en Recordatorios con debounce | Resultados tras 300ms de inactividad |
| 16 | Cambiar filtro de categoría | Tabla actualizada (tras fix) |
| 17 | Paginar tabla de Gastos | Navegación correcta entre páginas |
| 18 | Limpiar filtros en Proyectos | Todos los proyectos visibles |

### 3.4 Acciones Rápidas

| # | Caso de Prueba | Resultado Esperado |
|---|---------------|-------------------|
| 19 | Completar recordatorio desde listado | Snackbar confirmación, item desaparece o cambia estado |
| 20 | Pausar/Reanudar recordatorio | Estado cambia visualmente |
| 21 | Posponer alerta en Monitor | Diálogo abre, fecha guardada |
| 22 | Archivar proyecto desde detalle | Estado cambia a "Archivado" |

### 3.5 Navegación

| # | Caso de Prueba | Resultado Esperado |
|---|---------------|-------------------|
| 23 | Click en enlace de título en tabla | Navegar a vista de detalle |
| 24 | Botón "Volver" en formularios | Retorno a listado correspondiente |
| 25 | Deep link a `/backoffice/gastos/{id}` | Mostrar detalle correcto |
| 26 | URL inexistente `/backoffice/xyz` | Página 404 o redirección |

### 3.6 Estados de Loading

| # | Caso de Prueba | Resultado Esperado |
|---|---------------|-------------------|
| 27 | Cargar página de Dashboard | Progress bar visible durante carga |
| 28 | Guardar formulario lento | Spinner en botón, botón disabled |
| 29 | Reconectar tras pérdida de conexión | Modal de reconexión visible |

---

## 4. Mejoras Sugeridas (Sin Re-arquitectura)

### 4.1 Alta Prioridad (Bugs Funcionales)

1. **Cambiar AuthService a Scoped** - Fix crítico para aislamiento de sesiones
2. **Agregar InvokeAsync en MainLayout.OnLocationChanged** - Evitar race conditions
3. **Agregar estados de loading individuales en acciones** - Prevenir doble-click

### 4.2 Media Prioridad (UX)

4. **Sincronizar filtros automáticamente con LoadDataAsync** - Mejor experiencia de filtrado
5. **Agregar Required a MudDatePicker críticos** - Validación completa
6. **Reemplazar "usuario" hardcodeado por CurrentUserService** - Auditoría correcta

### 4.3 Baja Prioridad (Polish)

7. **Configurar opciones de MudDialog** - Evitar cierres accidentales
8. **Implementar CancellationToken en búsquedas** - Evitar race conditions en filtros
9. **Mejorar Home.razor para usuarios no autenticados** - UX más clara

---

## 5. Resumen Ejecutivo

| Severidad | Cantidad | Estado |
|-----------|----------|--------|
| **Crítico** | 1 | Requiere fix inmediato (AuthService Singleton) |
| **Alto** | 3 | Requiere fix antes de producción |
| **Medio** | 7 | Recomendado corregir |
| **Bajo** | 4 | Nice-to-have |
| **Sospecha** | 1 | Requiere validación adicional |

### Impacto Principal

El bug crítico del **AuthService como Singleton** hace que la aplicación sea **inutilizable en un entorno multi-usuario real**. Todos los usuarios comparten la misma sesión. Este debe ser el primer fix a implementar.

Los bugs de alta severidad relacionados con navegación y redirección pueden causar experiencias confusas pero no bloquean el uso de la aplicación.

---

*Generado automáticamente - Auditoría UI JSCHUB v1.0*
