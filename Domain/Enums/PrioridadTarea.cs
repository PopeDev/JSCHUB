namespace JSCHUB.Domain.Enums;

/// <summary>
/// Nivel de prioridad de una tarea Kanban
/// </summary>
public enum PrioridadTarea
{
    /// <summary>
    /// Prioridad baja - puede esperar
    /// </summary>
    Baja = 0,

    /// <summary>
    /// Prioridad media - importancia normal
    /// </summary>
    Media = 1,

    /// <summary>
    /// Prioridad alta - requiere atención pronto
    /// </summary>
    Alta = 2,

    /// <summary>
    /// Urgente - requiere atención inmediata
    /// </summary>
    Urgente = 3
}
