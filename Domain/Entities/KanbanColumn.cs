namespace JSCHUB.Domain.Entities;

/// <summary>
/// Columna del tablero Kanban de un proyecto
/// </summary>
public class KanbanColumn
{
    public Guid Id { get; set; }

    /// <summary>
    /// Proyecto al que pertenece esta columna
    /// </summary>
    public Guid ProyectoId { get; set; }

    /// <summary>
    /// Título de la columna (ej: "En curso", "Realizada")
    /// </summary>
    public string Titulo { get; set; } = string.Empty;

    /// <summary>
    /// Posición de la columna (menor = más a la izquierda)
    /// </summary>
    public int Posicion { get; set; } = 0;

    // Auditoría
    public string CreadoPor { get; set; } = "sistema";
    public DateTime CreadoEl { get; set; } = DateTime.UtcNow;
    public string ModificadoPor { get; set; } = "sistema";
    public DateTime ModificadoEl { get; set; } = DateTime.UtcNow;

    // Navegación
    public Proyecto Proyecto { get; set; } = null!;
    public ICollection<KanbanTask> Tareas { get; set; } = [];
}
