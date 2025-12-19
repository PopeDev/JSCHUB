# Guía de Estilo y Revisión

## Principios Generales
- **KISS**: Keep It Simple, Stupid.
- **DRY**: Don't Repeat Yourself.
- **YAGNI**: You Aren't Gonna Need It.

## Convenciones C#
- `PascalCase` para clases, métodos, propiedades y nombres de archivos.
- `camelCase` para variables locales y argumentos.
- `_camelCase` para campos privados.
- `IInterfaceName` para interfaces.

## Reglas de Revisión
### Arquitectura

* [ ] **La separación de capas se respeta** porque cada capa tiene una responsabilidad clara y las dependencias van solo “hacia fuera” (Domain no depende de nadie; Application depende de Domain; Infrastructure contiene los detalles; la UI consume Application).
* [ ] **Domain es independiente de frameworks** porque no referencia Blazor, EF Core, Npgsql ni usa atributos de persistencia; se limita a entidades/VO y reglas de negocio.
* [ ] **Application orquesta pero no implementa persistencia** porque define casos de uso y coordina el flujo mediante interfaces (repositorios/UnitOfWork), delegando la implementación de acceso a datos en Infrastructure.

### Blazor

* [ ] **Los componentes están divididos en UI lógica y vistas** porque `Pages` se usa para rutas y composición de pantalla, mientras `Components` agrupa piezas reutilizables parametrizables, evitando concentrar lógica en las páginas.
* [ ] **Se usa `OnInitializedAsync` en lugar de constructores** porque la inicialización y carga asíncrona de datos se realiza en el ciclo de vida del componente (`OnInitializedAsync`/`OnParametersSetAsync`), manteniendo los constructores ligeros y sin trabajo async.
* [ ] **Los servicios se inyectan vía `[Inject]` o constructor** porque se sigue el patrón estándar de DI en Blazor (atributo en `.razor` o constructor en code-behind), evitando patrones tipo `ServiceLocator`.

### Rendimiento

* [ ] **Las consultas a BD están optimizadas y evitan N+1** porque las lecturas se hacen con proyecciones a DTO/consultas agregadas, `Include` controlados o estrategias explícitas, además de paginación e índices cuando aplica, minimizando rondas a PostgreSQL.
* [ ] **El uso de `async/await` es correcto** porque el flujo es asíncrono de punta a punta (EF `*Async`, casos de uso async y handlers/lifecycle async en Blazor) y se evita bloquear con `.Result` o `.Wait()` para prevenir deadlocks y congelaciones de UI.
