using JSCHUB.Domain.Enums;

namespace JSCHUB.Domain.Entities;

/// <summary>
/// Tarea dentro de una columna del tablero Kanban
/// </summary>
public class KanbanTask
{
    public Guid Id { get; set; }

    /// <summary>
    /// Proyecto al que pertenece esta tarea
    /// </summary>
    public Guid ProyectoId { get; set; }

    /// <summary>
    /// Columna en la que se encuentra la tarea
    /// </summary>
    public Guid ColumnaId { get; set; }

    /// <summary>
    /// Título de la tarea (obligatorio)
    /// </summary>
    public string Titulo { get; set; } = string.Empty;

    /// <summary>
    /// Descripción detallada (opcional)
    /// </summary>
    public string? Descripcion { get; set; }

    /// <summary>
    /// Usuario asignado a la tarea (opcional, 0 o 1)
    /// </summary>
    public Guid? AsignadoAId { get; set; }

    /// <summary>
    /// Nivel de prioridad de la tarea
    /// </summary>
    public PrioridadTarea Prioridad { get; set; } = PrioridadTarea.Media;

    /// <summary>
    /// Horas estimadas para completar la tarea
    /// </summary>
    public decimal HorasEstimadas { get; set; } = 0;

    /// <summary>
    /// Posición dentro de la columna (menor = más arriba)
    /// </summary>
    public int Posicion { get; set; } = 0;

    // === Sprint ===

    /// <summary>
    /// Sprint al que pertenece esta tarea (null si no hay sprint activo)
    /// </summary>
    public Guid? SprintId { get; set; }

    /// <summary>
    /// Sprint donde se creó originalmente la tarea
    /// </summary>
    public Guid? SprintOrigenId { get; set; }

    /// <summary>
    /// True si la tarea estaba al inicio del sprint (comprometida)
    /// </summary>
    public bool EsComprometida { get; set; } = false;

    /// <summary>
    /// Número de sprints que ha pasado esta tarea sin completarse.
    /// Si > 2, se considera una alerta.
    /// </summary>
    public int SprintsTranscurridos { get; set; } = 0;

    // Auditoría
    public string CreadoPor { get; set; } = "sistema";
    public DateTime CreadoEl { get; set; } = DateTime.UtcNow;
    public string ModificadoPor { get; set; } = "sistema";
    public DateTime ModificadoEl { get; set; } = DateTime.UtcNow;

    // Navegación
    public Proyecto Proyecto { get; set; } = null!;
    public KanbanColumn Columna { get; set; } = null!;
    public Usuario? AsignadoA { get; set; }
    public Sprint? Sprint { get; set; }
}
