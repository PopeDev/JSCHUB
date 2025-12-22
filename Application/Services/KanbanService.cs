using JSCHUB.Application.DTOs;
using JSCHUB.Application.Interfaces;
using JSCHUB.Domain.Entities;
using JSCHUB.Domain.Enums;
using JSCHUB.Domain.Interfaces;
using Microsoft.Extensions.Logging;

namespace JSCHUB.Application.Services;

public class KanbanService : IKanbanService
{
    private readonly IKanbanRepository _repository;
    private readonly IProyectoRepository _proyectoRepository;
    private readonly ILogger<KanbanService> _logger;

    private static readonly string[] DefaultColumnTitles = ["Planificada", "En curso", "Realizada", "Desplegada"];

    public KanbanService(
        IKanbanRepository repository,
        IProyectoRepository proyectoRepository,
        ILogger<KanbanService> logger)
    {
        _repository = repository;
        _proyectoRepository = proyectoRepository;
        _logger = logger;
    }

    // =====================
    // Tablero
    // =====================

    public async Task<KanbanBoardDto> GetBoardAsync(Guid proyectoId, CancellationToken ct = default)
    {
        var proyecto = await _proyectoRepository.GetByIdAsync(proyectoId, ct)
            ?? throw new KeyNotFoundException($"No se encontró el proyecto con ID {proyectoId}");

        var columnas = await _repository.GetColumnsByProyectoIdWithTasksAsync(proyectoId, ct);

        return new KanbanBoardDto(
            proyecto.Id,
            proyecto.Nombre,
            columnas.Select(MapColumnToDto)
        );
    }

    public async Task EnsureDefaultColumnsAsync(Guid proyectoId, string usuario, CancellationToken ct = default)
    {
        var proyecto = await _proyectoRepository.GetByIdAsync(proyectoId, ct)
            ?? throw new KeyNotFoundException($"No se encontró el proyecto con ID {proyectoId}");

        var columnCount = await _repository.CountColumnsByProyectoIdAsync(proyectoId, ct);
        if (columnCount > 0)
            return; // Ya tiene columnas

        _logger.LogInformation("Creando columnas por defecto para proyecto {ProyectoId}", proyectoId);

        for (int i = 0; i < DefaultColumnTitles.Length; i++)
        {
            var column = new KanbanColumn
            {
                Id = Guid.NewGuid(),
                ProyectoId = proyectoId,
                Titulo = DefaultColumnTitles[i],
                Posicion = i,
                CreadoPor = usuario,
                CreadoEl = DateTime.UtcNow,
                ModificadoPor = usuario,
                ModificadoEl = DateTime.UtcNow
            };
            await _repository.AddColumnAsync(column, ct);
        }
    }

    // =====================
    // Columnas
    // =====================

    public async Task<KanbanColumnDto?> GetColumnByIdAsync(Guid id, CancellationToken ct = default)
    {
        var column = await _repository.GetColumnByIdWithTasksAsync(id, ct);
        return column is null ? null : MapColumnToDto(column);
    }

    public async Task<KanbanColumnDto> CreateColumnAsync(CreateKanbanColumnDto dto, string usuario, CancellationToken ct = default)
    {
        var proyecto = await _proyectoRepository.GetByIdAsync(dto.ProyectoId, ct)
            ?? throw new KeyNotFoundException($"No se encontró el proyecto con ID {dto.ProyectoId}");

        if (string.IsNullOrWhiteSpace(dto.Titulo))
            throw new ArgumentException("El título de la columna es obligatorio");

        var maxPosition = await _repository.GetMaxColumnPositionAsync(dto.ProyectoId, ct);

        var column = new KanbanColumn
        {
            Id = Guid.NewGuid(),
            ProyectoId = dto.ProyectoId,
            Titulo = dto.Titulo.Trim(),
            Posicion = dto.Posicion ?? (maxPosition + 1),
            CreadoPor = usuario,
            CreadoEl = DateTime.UtcNow,
            ModificadoPor = usuario,
            ModificadoEl = DateTime.UtcNow
        };

        await _repository.AddColumnAsync(column, ct);
        _logger.LogInformation("Columna Kanban creada: {Id} - {Titulo} en proyecto {ProyectoId}",
            column.Id, column.Titulo, column.ProyectoId);

        column.Tareas = new List<KanbanTask>();
        return MapColumnToDto(column);
    }

    public async Task<KanbanColumnDto> RenameColumnAsync(Guid id, string nuevoTitulo, string usuario, CancellationToken ct = default)
    {
        var column = await _repository.GetColumnByIdWithTasksAsync(id, ct)
            ?? throw new KeyNotFoundException($"No se encontró la columna con ID {id}");

        if (string.IsNullOrWhiteSpace(nuevoTitulo))
            throw new ArgumentException("El título de la columna es obligatorio");

        column.Titulo = nuevoTitulo.Trim();
        column.ModificadoPor = usuario;
        column.ModificadoEl = DateTime.UtcNow;

        await _repository.UpdateColumnAsync(column, ct);
        _logger.LogInformation("Columna Kanban renombrada: {Id} -> {Titulo}", column.Id, column.Titulo);

        return MapColumnToDto(column);
    }

    public async Task DeleteColumnAsync(Guid id, Guid? columnaDestinoId, CancellationToken ct = default)
    {
        var column = await _repository.GetColumnByIdWithTasksAsync(id, ct)
            ?? throw new KeyNotFoundException($"No se encontró la columna con ID {id}");

        var taskCount = column.Tareas?.Count ?? 0;

        if (taskCount > 0)
        {
            if (!columnaDestinoId.HasValue)
                throw new InvalidOperationException($"La columna tiene {taskCount} tarea(s). Debe especificar una columna destino para moverlas.");

            var destinoColumn = await _repository.GetColumnByIdAsync(columnaDestinoId.Value, ct)
                ?? throw new KeyNotFoundException($"No se encontró la columna destino con ID {columnaDestinoId}");

            if (destinoColumn.ProyectoId != column.ProyectoId)
                throw new InvalidOperationException("La columna destino debe pertenecer al mismo proyecto");

            await _repository.MoveTasksToColumnAsync(id, columnaDestinoId.Value, ct);
            _logger.LogInformation("Movidas {Count} tareas de columna {From} a {To}", taskCount, id, columnaDestinoId);
        }

        await _repository.DeleteColumnAsync(column, ct);
        _logger.LogInformation("Columna Kanban eliminada: {Id} - {Titulo}", column.Id, column.Titulo);
    }

    public async Task ReorderColumnsAsync(Guid proyectoId, IEnumerable<Guid> columnIdsOrdenados, string usuario, CancellationToken ct = default)
    {
        var columns = (await _repository.GetColumnsByProyectoIdAsync(proyectoId, ct)).ToList();
        var idsArray = columnIdsOrdenados.ToArray();

        for (int i = 0; i < idsArray.Length; i++)
        {
            var column = columns.FirstOrDefault(c => c.Id == idsArray[i]);
            if (column != null)
            {
                column.Posicion = i;
                column.ModificadoPor = usuario;
                column.ModificadoEl = DateTime.UtcNow;
            }
        }

        await _repository.UpdateColumnsPositionsAsync(columns, ct);
        _logger.LogInformation("Columnas reordenadas en proyecto {ProyectoId}", proyectoId);
    }

    public async Task MoveColumnAsync(Guid columnId, int newPosition, string usuario, CancellationToken ct = default)
    {
        var column = await _repository.GetColumnByIdAsync(columnId, ct)
            ?? throw new KeyNotFoundException($"No se encontró la columna con ID {columnId}");

        var columns = (await _repository.GetColumnsByProyectoIdAsync(column.ProyectoId, ct))
            .OrderBy(c => c.Posicion)
            .ToList();

        var oldPosition = column.Posicion;
        if (newPosition == oldPosition)
            return;

        // Ajustar posición válida
        newPosition = Math.Max(0, Math.Min(newPosition, columns.Count - 1));

        // Reordenar
        columns.Remove(column);
        columns.Insert(newPosition, column);

        for (int i = 0; i < columns.Count; i++)
        {
            columns[i].Posicion = i;
            columns[i].ModificadoPor = usuario;
            columns[i].ModificadoEl = DateTime.UtcNow;
        }

        await _repository.UpdateColumnsPositionsAsync(columns, ct);
        _logger.LogInformation("Columna {ColumnId} movida de posición {Old} a {New}", columnId, oldPosition, newPosition);
    }

    // =====================
    // Tareas
    // =====================

    public async Task<KanbanTaskDto?> GetTaskByIdAsync(Guid id, CancellationToken ct = default)
    {
        var task = await _repository.GetTaskByIdWithRelationsAsync(id, ct);
        return task is null ? null : MapTaskToDto(task);
    }

    public async Task<KanbanTaskDto> CreateTaskAsync(CreateKanbanTaskDto dto, string usuario, CancellationToken ct = default)
    {
        var column = await _repository.GetColumnByIdAsync(dto.ColumnaId, ct)
            ?? throw new KeyNotFoundException($"No se encontró la columna con ID {dto.ColumnaId}");

        if (string.IsNullOrWhiteSpace(dto.Titulo))
            throw new ArgumentException("El título de la tarea es obligatorio");

        var maxPosition = await _repository.GetMaxTaskPositionInColumnAsync(dto.ColumnaId, ct);

        var task = new KanbanTask
        {
            Id = Guid.NewGuid(),
            ProyectoId = dto.ProyectoId,
            ColumnaId = dto.ColumnaId,
            Titulo = dto.Titulo.Trim(),
            Descripcion = dto.Descripcion?.Trim(),
            AsignadoAId = dto.AsignadoAId,
            Prioridad = dto.Prioridad ?? PrioridadTarea.Media,
            HorasEstimadas = dto.HorasEstimadas ?? 0,
            Posicion = dto.Posicion ?? (maxPosition + 1),
            CreadoPor = usuario,
            CreadoEl = DateTime.UtcNow,
            ModificadoPor = usuario,
            ModificadoEl = DateTime.UtcNow
        };

        await _repository.AddTaskAsync(task, ct);
        _logger.LogInformation("Tarea Kanban creada: {Id} - {Titulo} en columna {ColumnaId}",
            task.Id, task.Titulo, task.ColumnaId);

        // Recargar con relaciones
        task = await _repository.GetTaskByIdWithRelationsAsync(task.Id, ct);
        return MapTaskToDto(task!);
    }

    public async Task<KanbanTaskDto> UpdateTaskAsync(Guid id, UpdateKanbanTaskDto dto, string usuario, CancellationToken ct = default)
    {
        var task = await _repository.GetTaskByIdWithRelationsAsync(id, ct)
            ?? throw new KeyNotFoundException($"No se encontró la tarea con ID {id}");

        if (string.IsNullOrWhiteSpace(dto.Titulo))
            throw new ArgumentException("El título de la tarea es obligatorio");

        task.Titulo = dto.Titulo.Trim();
        task.Descripcion = dto.Descripcion?.Trim();
        task.AsignadoAId = dto.AsignadoAId;
        task.Prioridad = dto.Prioridad;
        task.HorasEstimadas = dto.HorasEstimadas;
        task.ModificadoPor = usuario;
        task.ModificadoEl = DateTime.UtcNow;

        await _repository.UpdateTaskAsync(task, ct);
        _logger.LogInformation("Tarea Kanban actualizada: {Id} - {Titulo}", task.Id, task.Titulo);

        // Recargar con relaciones
        task = await _repository.GetTaskByIdWithRelationsAsync(task.Id, ct);
        return MapTaskToDto(task!);
    }

    public async Task DeleteTaskAsync(Guid id, CancellationToken ct = default)
    {
        var task = await _repository.GetTaskByIdAsync(id, ct)
            ?? throw new KeyNotFoundException($"No se encontró la tarea con ID {id}");

        await _repository.DeleteTaskAsync(task, ct);
        _logger.LogInformation("Tarea Kanban eliminada: {Id} - {Titulo}", task.Id, task.Titulo);
    }

    public async Task<KanbanTaskDto> MoveTaskAsync(Guid taskId, Guid toColumnaId, int? toPosition, string usuario, CancellationToken ct = default)
    {
        var task = await _repository.GetTaskByIdAsync(taskId, ct)
            ?? throw new KeyNotFoundException($"No se encontró la tarea con ID {taskId}");

        var destColumn = await _repository.GetColumnByIdAsync(toColumnaId, ct)
            ?? throw new KeyNotFoundException($"No se encontró la columna destino con ID {toColumnaId}");

        if (destColumn.ProyectoId != task.ProyectoId)
            throw new InvalidOperationException("La columna destino debe pertenecer al mismo proyecto");

        var oldColumnaId = task.ColumnaId;
        var maxPosition = await _repository.GetMaxTaskPositionInColumnAsync(toColumnaId, ct);

        task.ColumnaId = toColumnaId;
        task.Posicion = toPosition ?? (maxPosition + 1);
        task.ModificadoPor = usuario;
        task.ModificadoEl = DateTime.UtcNow;

        await _repository.UpdateTaskAsync(task, ct);
        _logger.LogInformation("Tarea {TaskId} movida de columna {From} a {To}", taskId, oldColumnaId, toColumnaId);

        // Recargar con relaciones
        task = await _repository.GetTaskByIdWithRelationsAsync(task.Id, ct);
        return MapTaskToDto(task!);
    }

    // =====================
    // Mapeo
    // =====================

    private static KanbanColumnDto MapColumnToDto(KanbanColumn c) => new(
        c.Id,
        c.ProyectoId,
        c.Titulo,
        c.Posicion,
        c.CreadoPor,
        c.CreadoEl,
        c.ModificadoPor,
        c.ModificadoEl,
        c.Tareas?.OrderBy(t => t.Posicion).Select(MapTaskToDto) ?? Enumerable.Empty<KanbanTaskDto>()
    );

    private static KanbanTaskDto MapTaskToDto(KanbanTask t) => new()
    {
        Id = t.Id,
        ProyectoId = t.ProyectoId,
        ColumnaId = t.ColumnaId,
        Titulo = t.Titulo,
        Descripcion = t.Descripcion,
        AsignadoAId = t.AsignadoAId,
        AsignadoANombre = t.AsignadoA?.Nombre,
        Prioridad = t.Prioridad,
        HorasEstimadas = t.HorasEstimadas,
        Posicion = t.Posicion,
        CreadoPor = t.CreadoPor,
        CreadoEl = t.CreadoEl,
        ModificadoPor = t.ModificadoPor,
        ModificadoEl = t.ModificadoEl
    };
}
