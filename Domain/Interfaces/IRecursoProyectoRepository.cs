using JSCHUB.Domain.Entities;
using JSCHUB.Domain.Enums;

namespace JSCHUB.Domain.Interfaces;

public interface IRecursoProyectoRepository
{
    Task<RecursoProyecto?> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<IEnumerable<RecursoProyecto>> GetByProyectoIdAsync(Guid proyectoId, CancellationToken ct = default);

    Task<IEnumerable<RecursoProyecto>> SearchAsync(
        Guid proyectoId,
        string? searchText = null,
        TipoRecurso? tipo = null,
        string? etiqueta = null,
        CancellationToken ct = default);

    Task<RecursoProyecto> AddAsync(RecursoProyecto recurso, CancellationToken ct = default);
    Task UpdateAsync(RecursoProyecto recurso, CancellationToken ct = default);
    Task DeleteAsync(RecursoProyecto recurso, CancellationToken ct = default);
}
