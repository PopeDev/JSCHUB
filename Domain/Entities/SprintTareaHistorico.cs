namespace JSCHUB.Domain.Entities;

/// <summary>
/// Snapshot histórico de una tarea al momento de cerrar un sprint.
/// Permite ver qué tareas se completaron en cada sprint.
/// </summary>
public class SprintTareaHistorico
{
    public Guid Id { get; set; }

    /// <summary>
    /// Sprint al que pertenece este registro histórico
    /// </summary>
    public Guid SprintId { get; set; }

    /// <summary>
    /// ID de la tarea original (puede haber sido eliminada posteriormente)
    /// </summary>
    public Guid TareaId { get; set; }

    /// <summary>
    /// Título de la tarea al momento del cierre
    /// </summary>
    public string TareaTitulo { get; set; } = string.Empty;

    /// <summary>
    /// Descripción de la tarea al momento del cierre
    /// </summary>
    public string? TareaDescripcion { get; set; }

    /// <summary>
    /// Nombre del usuario asignado al momento del cierre
    /// </summary>
    public string? AsignadoANombre { get; set; }

    /// <summary>
    /// True si la tarea estaba en la columna final (entregada)
    /// </summary>
    public bool FueEntregada { get; set; }

    /// <summary>
    /// Nombre de la columna donde estaba la tarea al cerrar
    /// </summary>
    public string? ColumnaFinal { get; set; }

    /// <summary>
    /// True si la tarea era comprometida (estaba al inicio del sprint)
    /// </summary>
    public bool EraComprometida { get; set; }

    /// <summary>
    /// Número de sprints que llevaba esta tarea sin completarse
    /// </summary>
    public int SprintsTranscurridos { get; set; }

    /// <summary>
    /// Fecha del registro
    /// </summary>
    public DateTime FechaRegistro { get; set; } = DateTime.UtcNow;

    // === Navegación ===
    public Sprint Sprint { get; set; } = null!;
}
