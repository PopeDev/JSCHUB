using JSCHUB.Domain.Entities;

namespace JSCHUB.Domain.Interfaces;

/// <summary>
/// Repositorio para gestión de Tools (herramientas de IA)
/// </summary>
public interface IToolRepository
{
    Task<Tool?> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<Tool?> GetByNameAsync(string name, CancellationToken ct = default);
    Task<IEnumerable<Tool>> GetAllAsync(CancellationToken ct = default);
    Task<IEnumerable<Tool>> GetActivosAsync(CancellationToken ct = default);
    Task<Tool> AddAsync(Tool tool, CancellationToken ct = default);
    Task UpdateAsync(Tool tool, CancellationToken ct = default);
    Task DeleteAsync(Tool tool, CancellationToken ct = default);
    Task<bool> ExistsAsync(Guid id, CancellationToken ct = default);
}
