using JSCHUB.Application.DTOs;

namespace JSCHUB.Application.Interfaces;

public interface IKanbanService
{
    // Tablero
    Task<KanbanBoardDto> GetBoardAsync(Guid proyectoId, CancellationToken ct = default);
    Task EnsureDefaultColumnsAsync(Guid proyectoId, string usuario, CancellationToken ct = default);

    // Columnas
    Task<KanbanColumnDto?> GetColumnByIdAsync(Guid id, CancellationToken ct = default);
    Task<KanbanColumnDto> CreateColumnAsync(CreateKanbanColumnDto dto, string usuario, CancellationToken ct = default);
    Task<KanbanColumnDto> RenameColumnAsync(Guid id, string nuevoTitulo, string usuario, CancellationToken ct = default);
    Task DeleteColumnAsync(Guid id, Guid? columnaDestinoId, CancellationToken ct = default);
    Task ReorderColumnsAsync(Guid proyectoId, IEnumerable<Guid> columnIdsOrdenados, string usuario, CancellationToken ct = default);

    // Tareas
    Task<KanbanTaskDto?> GetTaskByIdAsync(Guid id, CancellationToken ct = default);
    Task<KanbanTaskDto> CreateTaskAsync(CreateKanbanTaskDto dto, string usuario, CancellationToken ct = default);
    Task<KanbanTaskDto> UpdateTaskAsync(Guid id, UpdateKanbanTaskDto dto, string usuario, CancellationToken ct = default);
    Task DeleteTaskAsync(Guid id, CancellationToken ct = default);
    Task<KanbanTaskDto> MoveTaskAsync(Guid taskId, Guid columnaDestinoId, int nuevaPosicion, string usuario, CancellationToken ct = default);
    Task ReorderTasksInColumnAsync(Guid columnId, IEnumerable<Guid> taskIdsOrdenados, string usuario, CancellationToken ct = default);
}
