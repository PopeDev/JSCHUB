using JSCHUB.Domain.Entities;
using JSCHUB.Domain.Enums;

namespace JSCHUB.Domain.Interfaces;

public interface IEnlaceProyectoRepository
{
    Task<EnlaceProyecto?> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<IEnumerable<EnlaceProyecto>> GetByProyectoIdAsync(Guid proyectoId, CancellationToken ct = default);

    Task<IEnumerable<EnlaceProyecto>> SearchAsync(
        Guid proyectoId,
        string? searchText = null,
        TipoEnlace? tipo = null,
        CancellationToken ct = default);

    Task<EnlaceProyecto> AddAsync(EnlaceProyecto enlace, CancellationToken ct = default);
    Task UpdateAsync(EnlaceProyecto enlace, CancellationToken ct = default);
    Task DeleteAsync(EnlaceProyecto enlace, CancellationToken ct = default);
    Task<int> GetMaxOrdenAsync(Guid proyectoId, CancellationToken ct = default);
}
