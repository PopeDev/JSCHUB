using JSCHUB.Domain.Entities;

namespace JSCHUB.Domain.Interfaces;

public interface IKanbanRepository
{
    // Columnas
    Task<KanbanColumn?> GetColumnByIdAsync(Guid id, CancellationToken ct = default);
    Task<KanbanColumn?> GetColumnByIdWithTasksAsync(Guid id, CancellationToken ct = default);
    Task<IEnumerable<KanbanColumn>> GetColumnsByProyectoIdAsync(Guid proyectoId, CancellationToken ct = default);
    Task<IEnumerable<KanbanColumn>> GetColumnsByProyectoIdWithTasksAsync(Guid proyectoId, CancellationToken ct = default);
    Task<int> CountColumnsByProyectoIdAsync(Guid proyectoId, CancellationToken ct = default);
    Task<int> GetMaxColumnPositionAsync(Guid proyectoId, CancellationToken ct = default);
    Task<KanbanColumn> AddColumnAsync(KanbanColumn column, CancellationToken ct = default);
    Task UpdateColumnAsync(KanbanColumn column, CancellationToken ct = default);
    Task DeleteColumnAsync(KanbanColumn column, CancellationToken ct = default);
    Task UpdateColumnsPositionsAsync(IEnumerable<KanbanColumn> columns, CancellationToken ct = default);

    // Tareas
    Task<KanbanTask?> GetTaskByIdAsync(Guid id, CancellationToken ct = default);
    Task<IEnumerable<KanbanTask>> GetTasksByColumnIdAsync(Guid columnId, CancellationToken ct = default);
    Task<int> CountTasksByColumnIdAsync(Guid columnId, CancellationToken ct = default);
    Task<int> GetMaxTaskPositionAsync(Guid columnId, CancellationToken ct = default);
    Task<KanbanTask> AddTaskAsync(KanbanTask task, CancellationToken ct = default);
    Task UpdateTaskAsync(KanbanTask task, CancellationToken ct = default);
    Task DeleteTaskAsync(KanbanTask task, CancellationToken ct = default);
    Task UpdateTasksPositionsAsync(IEnumerable<KanbanTask> tasks, CancellationToken ct = default);
    Task MoveTasksToColumnAsync(Guid sourceColumnId, Guid destColumnId, CancellationToken ct = default);
}
