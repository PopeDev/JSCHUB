using JSCHUB.Application.DTOs;

namespace JSCHUB.Application.Interfaces;

/// <summary>
/// Servicio para gestión de Tags (etiquetas)
/// </summary>
public interface ITagService
{
    Task<TagDto?> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<IEnumerable<TagDto>> GetAllAsync(CancellationToken ct = default);
    Task<IEnumerable<TagDto>> GetActivosAsync(CancellationToken ct = default);
    Task<TagDto> CreateAsync(CreateTagDto dto, CancellationToken ct = default);
    Task<TagDto> UpdateAsync(Guid id, UpdateTagDto dto, CancellationToken ct = default);
    Task ToggleActivoAsync(Guid id, CancellationToken ct = default);
    Task DeleteAsync(Guid id, CancellationToken ct = default);
}
