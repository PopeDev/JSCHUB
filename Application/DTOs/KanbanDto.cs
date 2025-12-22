using JSCHUB.Domain.Enums;

namespace JSCHUB.Application.DTOs;

// =====================
// DTOs de Columnas
// =====================

/// <summary>
/// DTO de lectura para una columna Kanban
/// </summary>
public record KanbanColumnDto(
    Guid Id,
    Guid ProyectoId,
    string Titulo,
    int Posicion,
    string CreadoPor,
    DateTime CreadoEl,
    string ModificadoPor,
    DateTime ModificadoEl,
    IEnumerable<KanbanTaskDto> Tareas
);

/// <summary>
/// DTO para crear una columna Kanban
/// </summary>
public record CreateKanbanColumnDto(
    Guid ProyectoId,
    string Titulo,
    int? Posicion
);

/// <summary>
/// DTO para actualizar el título de una columna Kanban
/// </summary>
public record UpdateKanbanColumnDto(
    string Titulo
);

// =====================
// DTOs de Tareas
// =====================

/// <summary>
/// DTO de lectura para una tarea Kanban
/// </summary>
public class KanbanTaskDto
{
    public Guid Id { get; set; }
    public Guid ProyectoId { get; set; }
    public Guid ColumnaId { get; set; }
    public string Titulo { get; set; } = string.Empty;
    public string? Descripcion { get; set; }
    public Guid? AsignadoAId { get; set; }
    public string? AsignadoANombre { get; set; }
    public PrioridadTarea Prioridad { get; set; }
    public decimal HorasEstimadas { get; set; }
    public int Posicion { get; set; }
    public string CreadoPor { get; set; } = string.Empty;
    public DateTime CreadoEl { get; set; }
    public string ModificadoPor { get; set; } = string.Empty;
    public DateTime ModificadoEl { get; set; }
}

/// <summary>
/// DTO para crear una tarea Kanban
/// </summary>
public record CreateKanbanTaskDto(
    Guid ProyectoId,
    Guid ColumnaId,
    string Titulo,
    string? Descripcion,
    Guid? AsignadoAId,
    PrioridadTarea? Prioridad,
    decimal? HorasEstimadas,
    int? Posicion
);

/// <summary>
/// DTO para actualizar una tarea Kanban
/// </summary>
public record UpdateKanbanTaskDto(
    string Titulo,
    string? Descripcion,
    Guid? AsignadoAId,
    PrioridadTarea Prioridad,
    decimal HorasEstimadas
);

/// <summary>
/// DTO para mover una tarea a otra columna
/// </summary>
public record MoveKanbanTaskDto(
    Guid ColumnaDestinoId,
    int Posicion
);

// =====================
// DTOs del Tablero
// =====================

/// <summary>
/// DTO que representa el tablero Kanban completo de un proyecto
/// </summary>
public record KanbanBoardDto(
    Guid ProyectoId,
    string ProyectoNombre,
    IEnumerable<KanbanColumnDto> Columnas
);
