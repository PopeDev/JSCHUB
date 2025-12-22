using JSCHUB.Domain.Entities;

namespace JSCHUB.Domain.Interfaces;

/// <summary>
/// Repositorio para gestión de Prompts
/// </summary>
public interface IPromptRepository
{
    Task<Prompt?> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<Prompt?> GetByIdCompleteAsync(Guid id, CancellationToken ct = default);
    Task<IEnumerable<Prompt>> GetAllAsync(CancellationToken ct = default);
    Task<IEnumerable<Prompt>> GetActivosAsync(CancellationToken ct = default);
    Task<IEnumerable<Prompt>> SearchAsync(
        string? searchText = null,
        Guid? toolId = null,
        Guid? proyectoId = null,
        Guid? tagId = null,
        bool incluirInactivos = false,
        int skip = 0,
        int take = 50,
        CancellationToken ct = default);
    Task<Prompt> AddAsync(Prompt prompt, CancellationToken ct = default);
    Task UpdateAsync(Prompt prompt, CancellationToken ct = default);
    Task DeleteAsync(Prompt prompt, CancellationToken ct = default);
    Task<bool> ExistsAsync(Guid id, CancellationToken ct = default);
    Task UpdateTagsAsync(Guid promptId, IEnumerable<Guid> tagIds, CancellationToken ct = default);
}
