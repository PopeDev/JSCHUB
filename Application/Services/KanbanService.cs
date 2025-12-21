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

        var columns = await _repository.GetColumnsByProyectoIdWithTasksAsync(proyectoId, ct);

        return new KanbanBoardDto(
            proyectoId,
            proyecto.Nombre,
            columns.Select(MapColumnToDto)
        );
    }

    public async Task EnsureDefaultColumnsAsync(Guid proyectoId, string usuario, CancellationToken ct = default)
    {
        var count = await _repository.CountColumnsByProyectoIdAsync(proyectoId, ct);
        if (count > 0)
            return;

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

        if (proyecto.Estado == EstadoProyecto.Archivado)
            throw new InvalidOperationException("No se pueden añadir columnas a un proyecto archivado");

        if (string.IsNullOrWhiteSpace(dto.Titulo))
            throw new ArgumentException("El título de la columna es obligatorio");

        var maxPos = await _repository.GetMaxColumnPositionAsync(dto.ProyectoId, ct);

        var column = new KanbanColumn
        {
            Id = Guid.NewGuid(),
            ProyectoId = dto.ProyectoId,
            Titulo = dto.Titulo.Trim(),
            Posicion = dto.Posicion ?? (maxPos + 1),
            CreadoPor = usuario,
            CreadoEl = DateTime.UtcNow,
            ModificadoPor = usuario,
            ModificadoEl = DateTime.UtcNow
        };

        await _repository.AddColumnAsync(column, ct);
        _logger.LogInformation("Columna Kanban creada: {Id} - {Titulo} en proyecto {ProyectoId}",
            column.Id, column.Titulo, column.ProyectoId);

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
        _logger.LogInformation("Columna Kanban renombrada: {Id} - {Titulo}", column.Id, column.Titulo);

        return MapColumnToDto(column);
    }

    public async Task DeleteColumnAsync(Guid id, Guid? columnaDestinoId, CancellationToken ct = default)
    {
        var column = await _repository.GetColumnByIdAsync(id, ct)
            ?? throw new KeyNotFoundException($"No se encontró la columna con ID {id}");

        var taskCount = await _repository.CountTasksByColumnIdAsync(id, ct);

        if (taskCount > 0)
        {
            if (!columnaDestinoId.HasValue)
                throw new InvalidOperationException(
                    $"La columna tiene {taskCount} tarea(s). Debe especificar una columna destino para mover las tareas.");

            var destColumn = await _repository.GetColumnByIdAsync(columnaDestinoId.Value, ct)
                ?? throw new KeyNotFoundException($"No se encontró la columna destino con ID {columnaDestinoId}");

            if (destColumn.ProyectoId != column.ProyectoId)
                throw new InvalidOperationException("La columna destino debe pertenecer al mismo proyecto");

            await _repository.MoveTasksToColumnAsync(id, columnaDestinoId.Value, ct);
            _logger.LogInformation("Movidas {Count} tareas de columna {SourceId} a {DestId}",
                taskCount, id, columnaDestinoId.Value);
        }

        await _repository.DeleteColumnAsync(column, ct);
        _logger.LogInformation("Columna Kanban eliminada: {Id} - {Titulo}", column.Id, column.Titulo);
    }

    public async Task ReorderColumnsAsync(Guid proyectoId, IEnumerable<Guid> columnIdsOrdenados, string usuario, CancellationToken ct = default)
    {
        var columns = (await _repository.GetColumnsByProyectoIdAsync(proyectoId, ct)).ToList();
        var orderedIds = columnIdsOrdenados.ToList();

        if (columns.Count != orderedIds.Count)
            throw new ArgumentException("El número de columnas no coincide");

        var now = DateTime.UtcNow;
        for (int i = 0; i < orderedIds.Count; i++)
        {
            var column = columns.FirstOrDefault(c => c.Id == orderedIds[i])
                ?? throw new KeyNotFoundException($"No se encontró la columna con ID {orderedIds[i]}");

            column.Posicion = i;
            column.ModificadoPor = usuario;
            column.ModificadoEl = now;
        }

        await _repository.UpdateColumnsPositionsAsync(columns, ct);
        _logger.LogInformation("Columnas reordenadas en proyecto {ProyectoId}", proyectoId);
    }

    // =====================
    // Tareas
    // =====================

    public async Task<KanbanTaskDto?> GetTaskByIdAsync(Guid id, CancellationToken ct = default)
    {
        var task = await _repository.GetTaskByIdAsync(id, ct);
        return task is null ? null : MapTaskToDto(task);
    }

    public async Task<KanbanTaskDto> CreateTaskAsync(CreateKanbanTaskDto dto, string usuario, CancellationToken ct = default)
    {
        var column = await _repository.GetColumnByIdAsync(dto.ColumnaId, ct)
            ?? throw new KeyNotFoundException($"No se encontró la columna con ID {dto.ColumnaId}");

        if (column.ProyectoId != dto.ProyectoId)
            throw new InvalidOperationException("La columna no pertenece al proyecto especificado");

        if (string.IsNullOrWhiteSpace(dto.Titulo))
            throw new ArgumentException("El título de la tarea es obligatorio");

        if (dto.HorasEstimadas.HasValue && dto.HorasEstimadas.Value < 0)
            throw new ArgumentException("Las horas estimadas no pueden ser negativas");

        var maxPos = await _repository.GetMaxTaskPositionAsync(dto.ColumnaId, ct);

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
            Posicion = dto.Posicion ?? (maxPos + 1),
            CreadoPor = usuario,
            CreadoEl = DateTime.UtcNow,
            ModificadoPor = usuario,
            ModificadoEl = DateTime.UtcNow
        };

        await _repository.AddTaskAsync(task, ct);
        _logger.LogInformation("Tarea Kanban creada: {Id} - {Titulo} en columna {ColumnaId}",
            task.Id, task.Titulo, task.ColumnaId);

        // Recargar para obtener el usuario asignado
        var createdTask = await _repository.GetTaskByIdAsync(task.Id, ct);
        return MapTaskToDto(createdTask!);
    }

    public async Task<KanbanTaskDto> UpdateTaskAsync(Guid id, UpdateKanbanTaskDto dto, string usuario, CancellationToken ct = default)
    {
        var task = await _repository.GetTaskByIdAsync(id, ct)
            ?? throw new KeyNotFoundException($"No se encontró la tarea con ID {id}");

        if (string.IsNullOrWhiteSpace(dto.Titulo))
            throw new ArgumentException("El título de la tarea es obligatorio");

        if (dto.HorasEstimadas < 0)
            throw new ArgumentException("Las horas estimadas no pueden ser negativas");

        task.Titulo = dto.Titulo.Trim();
        task.Descripcion = dto.Descripcion?.Trim();
        task.AsignadoAId = dto.AsignadoAId;
        task.Prioridad = dto.Prioridad;
        task.HorasEstimadas = dto.HorasEstimadas;
        task.ModificadoPor = usuario;
        task.ModificadoEl = DateTime.UtcNow;

        await _repository.UpdateTaskAsync(task, ct);
        _logger.LogInformation("Tarea Kanban actualizada: {Id} - {Titulo}", task.Id, task.Titulo);

        // Recargar para obtener el usuario asignado
        var updatedTask = await _repository.GetTaskByIdAsync(id, ct);
        return MapTaskToDto(updatedTask!);
    }

    public async Task DeleteTaskAsync(Guid id, CancellationToken ct = default)
    {
        var task = await _repository.GetTaskByIdAsync(id, ct)
            ?? throw new KeyNotFoundException($"No se encontró la tarea con ID {id}");

        await _repository.DeleteTaskAsync(task, ct);
        _logger.LogInformation("Tarea Kanban eliminada: {Id} - {Titulo}", task.Id, task.Titulo);
    }

    public async Task<KanbanTaskDto> MoveTaskAsync(Guid taskId, Guid columnaDestinoId, int nuevaPosicion, string usuario, CancellationToken ct = default)
    {
        var task = await _repository.GetTaskByIdAsync(taskId, ct)
            ?? throw new KeyNotFoundException($"No se encontró la tarea con ID {taskId}");

        var destColumn = await _repository.GetColumnByIdAsync(columnaDestinoId, ct)
            ?? throw new KeyNotFoundException($"No se encontró la columna destino con ID {columnaDestinoId}");

        if (destColumn.ProyectoId != task.ProyectoId)
            throw new InvalidOperationException("La columna destino debe pertenecer al mismo proyecto");

        var oldColumnId = task.ColumnaId;
        var oldPosition = task.Posicion;

        task.ColumnaId = columnaDestinoId;
        task.Posicion = nuevaPosicion;
        task.ModificadoPor = usuario;
        task.ModificadoEl = DateTime.UtcNow;

        // Reordenar tareas en la columna destino
        var destTasks = (await _repository.GetTasksByColumnIdAsync(columnaDestinoId, ct))
            .Where(t => t.Id != taskId)
            .OrderBy(t => t.Posicion)
            .ToList();

        // Insertar en la posición correcta
        var reorderedTasks = new List<KanbanTask>();
        int pos = 0;
        bool inserted = false;

        foreach (var t in destTasks)
        {
            if (pos == nuevaPosicion && !inserted)
            {
                task.Posicion = pos;
                reorderedTasks.Add(task);
                pos++;
                inserted = true;
            }
            t.Posicion = pos;
            t.ModificadoPor = usuario;
            t.ModificadoEl = DateTime.UtcNow;
            reorderedTasks.Add(t);
            pos++;
        }

        if (!inserted)
        {
            task.Posicion = pos;
            reorderedTasks.Add(task);
        }

        await _repository.UpdateTasksPositionsAsync(reorderedTasks, ct);

        // Reordenar tareas en la columna origen si es diferente
        if (oldColumnId != columnaDestinoId)
        {
            var sourceTasks = (await _repository.GetTasksByColumnIdAsync(oldColumnId, ct))
                .OrderBy(t => t.Posicion)
                .ToList();

            for (int i = 0; i < sourceTasks.Count; i++)
            {
                sourceTasks[i].Posicion = i;
                sourceTasks[i].ModificadoPor = usuario;
                sourceTasks[i].ModificadoEl = DateTime.UtcNow;
            }

            if (sourceTasks.Count > 0)
                await _repository.UpdateTasksPositionsAsync(sourceTasks, ct);
        }

        _logger.LogInformation("Tarea {TaskId} movida de columna {OldCol} a {NewCol} posición {NewPos}",
            taskId, oldColumnId, columnaDestinoId, nuevaPosicion);

        var movedTask = await _repository.GetTaskByIdAsync(taskId, ct);
        return MapTaskToDto(movedTask!);
    }

    public async Task ReorderTasksInColumnAsync(Guid columnId, IEnumerable<Guid> taskIdsOrdenados, string usuario, CancellationToken ct = default)
    {
        var tasks = (await _repository.GetTasksByColumnIdAsync(columnId, ct)).ToList();
        var orderedIds = taskIdsOrdenados.ToList();

        if (tasks.Count != orderedIds.Count)
            throw new ArgumentException("El número de tareas no coincide");

        var now = DateTime.UtcNow;
        for (int i = 0; i < orderedIds.Count; i++)
        {
            var task = tasks.FirstOrDefault(t => t.Id == orderedIds[i])
                ?? throw new KeyNotFoundException($"No se encontró la tarea con ID {orderedIds[i]}");

            task.Posicion = i;
            task.ModificadoPor = usuario;
            task.ModificadoEl = now;
        }

        await _repository.UpdateTasksPositionsAsync(tasks, ct);
        _logger.LogInformation("Tareas reordenadas en columna {ColumnId}", columnId);
    }

    // =====================
    // Mappers
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
        c.Tareas?.OrderBy(t => t.Posicion).Select(MapTaskToDto) ?? []
    );

    private static KanbanTaskDto MapTaskToDto(KanbanTask t) => new(
        t.Id,
        t.ProyectoId,
        t.ColumnaId,
        t.Titulo,
        t.Descripcion,
        t.AsignadoAId,
        t.AsignadoA?.Nombre,
        t.Prioridad,
        t.HorasEstimadas,
        t.Posicion,
        t.CreadoPor,
        t.CreadoEl,
        t.ModificadoPor,
        t.ModificadoEl
    );
}
