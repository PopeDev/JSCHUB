using JSCHUB.Domain.Entities;

namespace JSCHUB.Domain.Interfaces;

/// <summary>
/// Repositorio para gestión de Tags (etiquetas)
/// </summary>
public interface ITagRepository
{
    Task<Tag?> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<Tag?> GetByNameAsync(string name, CancellationToken ct = default);
    Task<IEnumerable<Tag>> GetAllAsync(CancellationToken ct = default);
    Task<IEnumerable<Tag>> GetActivosAsync(CancellationToken ct = default);
    Task<IEnumerable<Tag>> GetByIdsAsync(IEnumerable<Guid> ids, CancellationToken ct = default);
    Task<Tag> AddAsync(Tag tag, CancellationToken ct = default);
    Task UpdateAsync(Tag tag, CancellationToken ct = default);
    Task DeleteAsync(Tag tag, CancellationToken ct = default);
    Task<bool> ExistsAsync(Guid id, CancellationToken ct = default);
}
