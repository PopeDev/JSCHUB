using JSCHUB.Domain.Enums;

namespace JSCHUB.Domain.Entities;

/// <summary>
/// Representa un período de trabajo (Sprint) dentro de un proyecto.
/// Cada proyecto tiene un sprint activo al que se asocian las tareas.
/// </summary>
public class Sprint
{
    public Guid Id { get; set; }

    /// <summary>
    /// Proyecto al que pertenece este sprint
    /// </summary>
    public Guid ProyectoId { get; set; }

    /// <summary>
    /// Nombre del sprint (ej: "Sprint 1", "Sprint 2")
    /// </summary>
    public string Nombre { get; set; } = string.Empty;

    /// <summary>
    /// Etiqueta de temporalización humana (ej: "1er Trimestre Sprint 1")
    /// </summary>
    public string Temporizacion { get; set; } = string.Empty;

    /// <summary>
    /// Objetivo del sprint (texto descriptivo)
    /// </summary>
    public string? Objetivo { get; set; }

    /// <summary>
    /// Fecha de inicio del sprint
    /// </summary>
    public DateOnly FechaInicio { get; set; }

    /// <summary>
    /// Fecha de fin del sprint
    /// </summary>
    public DateOnly FechaFin { get; set; }

    /// <summary>
    /// Duración del sprint en días (calculada)
    /// </summary>
    public int DuracionDias => (FechaFin.ToDateTime(TimeOnly.MinValue) 
                               - FechaInicio.ToDateTime(TimeOnly.MinValue)).Days + 1;

    /// <summary>
    /// Estado actual del sprint
    /// </summary>
    public EstadoSprint Estado { get; set; } = EstadoSprint.Pendiente;

    // === Métricas de cierre (congeladas al cerrar) ===

    /// <summary>
    /// Número de tareas comprometidas al inicio del sprint
    /// </summary>
    public int? TareasComprometidas { get; set; }

    /// <summary>
    /// Número de tareas entregadas (en columna final) al cerrar
    /// </summary>
    public int? TareasEntregadas { get; set; }

    /// <summary>
    /// Porcentaje de completitud: (Entregadas / Comprometidas) * 100
    /// </summary>
    public decimal? PorcentajeCompletitud { get; set; }

    /// <summary>
    /// Fecha y hora en que se cerró el sprint
    /// </summary>
    public DateTime? FechaCierre { get; set; }

    // === Auditoría ===
    public string CreadoPor { get; set; } = "sistema";
    public DateTime CreadoEl { get; set; } = DateTime.UtcNow;
    public string ModificadoPor { get; set; } = "sistema";
    public DateTime ModificadoEl { get; set; } = DateTime.UtcNow;

    // === Navegación ===
    public Proyecto Proyecto { get; set; } = null!;
    public ICollection<KanbanTask> Tareas { get; set; } = [];
    public ICollection<SprintTareaHistorico> TareasHistorico { get; set; } = [];
}
