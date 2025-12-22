namespace JSCHUB.Domain.Enums;

/// <summary>
/// Nivel de prioridad de una tarea Kanban
/// </summary>
public enum PrioridadTarea
{
    /// <summary>
    /// Prioridad baja - puede esperar
    /// </summary>
    Baja,

    /// <summary>
    /// Prioridad media - importancia normal
    /// </summary>
    Media,

    /// <summary>
    /// Prioridad alta - requiere atención pronto
    /// </summary>
    Alta,

    /// <summary>
    /// Urgente - atención inmediata requerida
    /// </summary>
    Urgente
}
