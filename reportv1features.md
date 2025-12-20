# JSCHUB - Auditoría de Código v1.0

**Fecha:** 2025-12-19
**Auditor:** Claude Code
**Versión de la App:** 1.0 (Inicial)

---

## Resumen Ejecutivo

JSCHUB es una aplicación Blazor Server con .NET 10.0 que actualmente gestiona:
- **Recordatorios/Renovaciones** (dominios, hosting, impuestos, etc.)
- **Sistema de Alertas** automáticas con severidad
- **Gestión de Gastos** (estilo Splitwise simplificado)
- **Gestión de Proyectos** con enlaces y recursos

La aplicación sigue Clean Architecture y tiene una base sólida, pero presenta varias áreas de mejora y oportunidades de expansión.

---

## 1. BUGS Y PROBLEMAS CRÍTICOS

### 1.1 Autenticación Insegura (CRÍTICO)
**Archivo:** `Infrastructure/Services/AuthService.cs:15-20`

```csharp
private static readonly Dictionary<string, string> ValidCredentials = new(StringComparer.OrdinalIgnoreCase)
{
    ["Javi"] = "123456",
    ["Pope"] = "00000",
    ["Carlos"] = "010203"
};
```

**Problemas:**
- Contraseñas en texto plano hardcodeadas
- Sin hash de contraseñas (bcrypt, PBKDF2, Argon2)
- Sesión almacenada en memoria (singleton) - se pierde al reiniciar
- No hay persistencia de sesión entre requests
- No compatible con múltiples instancias (escalabilidad horizontal)
- No hay expiración de sesión

**Impacto:** Seguridad crítica, no apto para producción.

### 1.2 Nombres Duplicados de Proyecto No Bloqueados
**Archivo:** `Application/Services/ProyectoService.cs:61-65`

```csharp
var existeNombre = await _repository.ExisteNombreAsync(dto.Nombre, null, ct);
if (existeNombre)
{
    _logger.LogWarning("Intento de crear proyecto con nombre duplicado: {Nombre}", dto.Nombre);
    // ⚠️ NO LANZA EXCEPCIÓN - permite duplicados
}
```

**Problema:** Solo registra warning pero permite crear proyectos duplicados.
**Impacto:** Datos inconsistentes, confusión de usuarios.

### 1.3 Validación Duplicada (Servicio vs FluentValidation)
**Archivos:** `Application/Services/GastoService.cs:67-69` y `Application/Validators/GastoValidators.cs`

```csharp
// En el servicio:
if (dto.Importe <= 0)
    throw new ArgumentException("El importe debe ser mayor que 0");

// En el validador:
RuleFor(x => x.Importe)
    .GreaterThan(0).WithMessage("El importe debe ser mayor que 0");
```

**Problema:** Validación duplicada - el validador nunca se usa explícitamente en el servicio.
**Impacto:** Código redundante, posible inconsistencia en mensajes.

### 1.4 ResolvedToday Siempre es 0
**Archivo:** `Application/Services/AlertService.cs:64-65`

```csharp
// Para ResolvedToday necesitaríamos un método adicional, lo simplificamos
var resolvedToday = 0;
```

**Problema:** La estadística "Resueltas hoy" siempre muestra 0.
**Impacto:** Dashboard incompleto, métricas incorrectas.

### 1.5 Falta Eliminación Física (Solo Soft Delete)
**Archivos:** `Application/Services/ReminderService.cs:137-147`, `GastoService.cs:131-140`

- Recordatorios: `Status = ItemStatus.Archived` (soft delete)
- Gastos: `Estado = EstadoGasto.Anulado` (soft delete)

**Problema:** No hay opción para eliminar registros permanentemente.
**Impacto:** Acumulación de datos, posibles problemas de privacidad (GDPR).

### 1.6 Sin Confirmación para Acciones Destructivas
**Archivo:** `Components/Pages/Backoffice/Gastos.razor:124`

```csharp
<MudIconButton ... OnClick="@(() => AnularAsync(context.Id))" />
```

**Problema:** Al hacer clic en "Anular" se ejecuta inmediatamente sin diálogo de confirmación.
**Impacto:** Anulaciones accidentales.

---

## 2. MALAS PRÁCTICAS

### 2.1 Falta de Transacciones
**Archivos:** Todos los servicios

```csharp
// Ejemplo en ReminderService.CompleteAsync
foreach (var alert in item.Alerts.Where(...))
{
    alert.Resolve();
    await _alertRepository.UpdateAsync(alert, ct); // Cada alerta es una transacción separada
}
await _repository.UpdateAsync(item, ct);
```

**Problema:** Si falla en medio, quedan alertas resueltas con item sin completar.
**Solución:** Usar `IDbContextTransaction` o Unit of Work pattern.

### 2.2 Magic Strings para Estados y Acciones
**Archivos:** Múltiples

```csharp
await _auditService.LogAsync("ReminderItem", item.Id, "Create", ...);
await _auditService.LogAsync("Alert", id, "Acknowledge", ...);
```

**Problema:** Strings hardcodeados propensos a typos.
**Solución:** Usar constantes o enums para acciones de auditoría.

### 2.3 Mapeo Manual Repetitivo
**Archivos:** Todos los servicios (`MapToDto` métodos)

```csharp
private static GastoDto MapToDto(Gasto gasto) => new(
    gasto.Id,
    gasto.Concepto,
    // ... 10+ propiedades
);
```

**Problema:** Mantenimiento tedioso, propenso a errores si se añaden campos.
**Solución:** Usar AutoMapper o Mapster.

### 2.4 Falta de Paginación Real en UI
**Archivo:** `Components/Pages/Backoffice/Gastos.razor:131-133`

```razor
<MudTablePager PageSizeOptions="new int[] { 10, 25, 50, 100 }" />
```

**Problema:** El paginador existe pero los datos se cargan todos de golpe (`take: 100`).
**Impacto:** Problemas de rendimiento con muchos registros.

### 2.5 Timezone Hardcodeado
**Archivo:** `Domain/Entities/ReminderItem.cs:45`

```csharp
public string Timezone { get; set; } = "Europe/Madrid";
```

**Problema:** Default asume zona horaria española.
**Impacto:** Problemas para usuarios internacionales.

### 2.6 Sin Logging Estructurado Completo
**Archivos:** Servicios varios

Solo se loguea en algunas operaciones, no hay correlation ID, no hay trazabilidad completa.

---

## 3. INCONSISTENCIAS EN LÓGICA DE NEGOCIO

### 3.1 Estados de Gasto Incompletos
**Archivo:** `Domain/Enums/EstadoGasto.cs`

```csharp
public enum EstadoGasto
{
    Registrado,
    Anulado,
    Saldado,
    PendienteDevolucion
}
```

**Inconsistencias:**
- No hay flujo de estados claro (¿de Registrado se puede ir a todos?)
- `PendienteDevolucion` sugiere deudas pero no hay entidad Deuda
- No hay lógica de "saldar" implementada

### 3.2 Alertas No Se Re-abren al Despertar Snooze
**Archivo:** `Application/Services/AlertService.cs`

Las alertas pospuestas (`Snoozed`) no vuelven automáticamente a `Open` cuando pasa la fecha de snooze.

### 3.3 Persona vs Usuario: Conceptos Mezclados
- `Persona` en `Domain/Entities/Persona.cs` - usado para gastos
- Usuario de sesión en `AuthService` - credenciales hardcodeadas
- `Assignee` en `ReminderItem` - string libre

**Problema:** No hay relación clara entre quien paga gastos y quien usa el sistema.

### 3.4 Metadatos Sin Validación
**Archivo:** `Domain/Entities/ReminderItem.cs:57`

```csharp
public Dictionary<string, string> Metadata { get; set; } = new();
```

**Problema:** Sin esquema definido, cualquier clave/valor es válido.
**Riesgo:** Datos inconsistentes, difícil de consultar.

---

## 4. FUNCIONALIDADES FALTANTES

### 4.1 Sistema de Liquidación de Gastos
El modelo `Gasto` tiene estado `PendienteDevolucion` pero no hay:
- Cálculo de balances entre personas
- Registro de transferencias/pagos
- Historial de liquidaciones

### 4.2 Notificaciones
No hay sistema de notificaciones:
- Email cuando vence una alerta
- Push notifications
- Integración con Slack/Teams

### 4.3 Adjuntos/Documentos
```csharp
// TODO: Adjuntos - implementar más adelante
// public string? AdjuntoUrl { get; set; }
```

Los gastos deberían poder tener facturas adjuntas (PDF, imagen).

### 4.4 Multi-tenancy
Todo está en un solo contexto sin separación de datos por empresa/grupo.

### 4.5 Dashboard Analítico
- No hay gráficos de tendencias
- No hay resumen de gastos por persona/mes
- No hay KPIs de proyectos

### 4.6 Exportación de Datos
- No hay exportación a Excel/CSV
- No hay generación de reportes PDF

### 4.7 API REST
Solo existe UI Blazor, no hay API para integraciones externas.

### 4.8 Búsqueda Avanzada
- No hay búsqueda full-text en metadatos de recordatorios
- No hay filtros combinados complejos
- No hay búsqueda guardada

---

## 5. MEJORAS DE SEGURIDAD

### 5.1 Autenticación
- [ ] Implementar ASP.NET Core Identity
- [ ] Usar PasswordHasher<T> para hash de contraseñas
- [ ] Añadir 2FA (Two-Factor Authentication)
- [ ] Implementar bloqueo tras intentos fallidos
- [ ] Añadir refresh tokens

### 5.2 Autorización
- [ ] Implementar roles (Admin, Usuario, ReadOnly)
- [ ] Autorización por recurso (solo ver tus gastos)
- [ ] Políticas de autorización

### 5.3 Protección de Datos
- [ ] Cifrado de datos sensibles en BD
- [ ] Enmascaramiento de datos en logs
- [ ] Implementar audit trail completo
- [ ] Rate limiting

### 5.4 Validación
- [ ] Sanitización de inputs
- [ ] Validación de URLs en enlaces
- [ ] Prevención de inyección SQL (ya cubierto por EF Core)
- [ ] CSP headers

---

## 6. MEJORAS DE ARQUITECTURA

### 6.1 Capa de Aplicación
- [ ] Implementar CQRS (Command Query Responsibility Segregation)
- [ ] Usar MediatR para handlers
- [ ] Añadir comportamientos (logging, validación, transacciones)

### 6.2 Capa de Infraestructura
- [ ] Implementar Unit of Work pattern
- [ ] Añadir caching (Redis)
- [ ] Implementar outbox pattern para eventos

### 6.3 Testing
- [ ] Unit tests para servicios
- [ ] Integration tests para repositorios
- [ ] E2E tests para flujos críticos
- [ ] Tests de carga

### 6.4 Observabilidad
- [ ] Implementar OpenTelemetry
- [ ] Añadir métricas de negocio
- [ ] Health checks
- [ ] Distributed tracing

---

## 7. PROPUESTAS DE AMPLIACIÓN

### 7.1 Módulo de Facturación
```
Facturas/
├── Entities/
│   ├── Factura.cs
│   ├── LineaFactura.cs
│   └── Cliente.cs
├── Features/
│   ├── Emisión de facturas
│   ├── Series de facturación
│   ├── Numeración automática
│   └── Generación PDF
```

### 7.2 Módulo de Tiempo
```
TimeTracking/
├── Entities/
│   ├── RegistroTiempo.cs
│   └── Tarea.cs
├── Features/
│   ├── Timer integrado
│   ├── Asociación proyecto-tarea-tiempo
│   ├── Reportes de horas
│   └── Facturación por horas
```

### 7.3 Módulo CRM Ligero
```
CRM/
├── Entities/
│   ├── Contacto.cs
│   ├── Empresa.cs
│   └── Oportunidad.cs
├── Features/
│   ├── Gestión de leads
│   ├── Pipeline de ventas
│   └── Historial de comunicaciones
```

### 7.4 Integraciones
- **Stripe/PayPal**: Pagos online
- **Google Calendar**: Sincronización de recordatorios
- **GitHub/GitLab**: Webhooks para proyectos
- **Slack/Discord**: Notificaciones
- **Email**: SMTP para alertas

### 7.5 Mobile App
- API REST para backend
- App móvil con .NET MAUI
- Notificaciones push nativas

---

## 8. ROADMAP SUGERIDO

### Fase 1: Estabilización (Prioridad Alta)
1. Implementar autenticación segura con Identity
2. Corregir bug de nombres duplicados en proyectos
3. Implementar confirmaciones para acciones destructivas
4. Añadir transacciones a operaciones multi-entidad
5. Completar estadística "Resueltas hoy"

### Fase 2: Funcionalidad Core (Prioridad Media)
1. Sistema de liquidación de gastos
2. Adjuntos para gastos
3. Exportación a Excel/CSV
4. Dashboard con gráficos básicos
5. Notificaciones por email

### Fase 3: Expansión (Prioridad Normal)
1. API REST
2. Módulo de facturación
3. Time tracking
4. Integración con calendario
5. Multi-idioma

### Fase 4: Escala (Futuro)
1. Multi-tenancy
2. Mobile app
3. Integraciones externas
4. Analytics avanzado

---

## 9. MÉTRICAS DE CALIDAD ACTUAL

| Métrica | Valor | Objetivo |
|---------|-------|----------|
| **Cobertura de Tests** | 0% | >80% |
| **Complejidad Ciclomática** | Baja-Media | Baja |
| **Deuda Técnica** | Media | Baja |
| **Documentación** | Básica | Completa |
| **Seguridad** | Crítica (auth) | Alta |
| **Mantenibilidad** | Buena | Excelente |

---

## 10. CONCLUSIÓN

**Fortalezas:**
- Arquitectura Clean bien estructurada
- Uso correcto de patrones (Repository, DI)
- Código legible y bien organizado
- MudBlazor proporciona buena UX
- Base sólida para crecer

**Debilidades Críticas:**
- Autenticación no apta para producción
- Falta de tests
- Sin transacciones en operaciones complejas
- Validadores FluentValidation no integrados

**Recomendación:** Priorizar la seguridad (autenticación) antes de cualquier otra mejora o ampliación. La aplicación tiene potencial para convertirse en una suite completa de gestión empresarial tipo ERP ligero.

---

*Informe generado automáticamente por Claude Code*
