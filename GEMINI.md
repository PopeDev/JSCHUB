# GEMINI.md - Contexto Persistente

Este archivo proporciona el contexto fundamental que Gemini debe tener siempre presente.

## 🧠 Memoria Central

### Arquitectura
El proyecto sigue **Clean Architecture** estricta.
- **Domain**: Núcleo agnóstico. Entidades y abstracciones.
- **Application**: Casos de uso, DTOs, validaciones.
- **Infrastructure**: Implementación de interfaces (DB, API externas).
- **Presentation**: UI con Blazor Server.

### Reglas de Oro (Styleguide Resumida)
1. **Async/Await**: Todo I/O debe ser asíncrono.
2. **Inyección de Dependencias**: Usar constructor injection para todo.
3. **Records**: Usar `record` para DTOs.
4. **Functional Core**: Preferir inmutabilidad donde sea posible.

## 📚 Referencias Rápidas
- **Reglas de Frontend**: Ver `agent/rules/frontend.md`
- **Seguridad**: Ver `agent/rules/security.md`
- **Comandos**: Ver `.gemini/commands/`

## 🔄 Flujos Comunes
Para ejecutar tareas complejas, invoca los workflows:
- `@agent/workflows/add-entity.md` - Nueva entidad
- `@agent/workflows/create-page.md` - Nueva página
- `@agent/workflows/migration.md` - Migración BD
