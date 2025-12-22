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
public record KanbanTaskDto(
    Guid Id,
    Guid ProyectoId,
    Guid ColumnaId,
    string Titulo,
    string? Descripcion,
    Guid? AsignadoAId,
    string? AsignadoANombre,
    PrioridadTarea Prioridad,
    decimal HorasEstimadas,
    int Posicion,
    string CreadoPor,
    DateTime CreadoEl,
    string ModificadoPor,
    DateTime ModificadoEl
);

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
