namespace JSCHUB.Domain.Enums;

/// <summary>
/// Estado del ciclo de vida de un Sprint
/// </summary>
public enum EstadoSprint
{
    /// <summary>
    /// Sprint planificado pero aún no iniciado
    /// </summary>
    Pendiente,

    /// <summary>
    /// Sprint en curso, es el sprint activo del proyecto
    /// </summary>
    Activo,

    /// <summary>
    /// Sprint finalizado con métricas congeladas
    /// </summary>
    Cerrado
}
