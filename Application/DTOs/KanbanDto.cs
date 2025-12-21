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
/// DTO simplificado de columna (sin tareas)
/// </summary>
public record KanbanColumnSimpleDto(
    Guid Id,
    Guid ProyectoId,
    string Titulo,
    int Posicion
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
/// DTO para actualizar una columna Kanban
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
/// DTO para mover una tarea a otra columna/posición
/// </summary>
public record MoveKanbanTaskDto(
    Guid ColumnaDestinoId,
    int NuevaPosicion
);

// =====================
// DTOs de Tablero
// =====================

/// <summary>
/// DTO del tablero completo de un proyecto
/// </summary>
public record KanbanBoardDto(
    Guid ProyectoId,
    string ProyectoNombre,
    IEnumerable<KanbanColumnDto> Columnas
);
