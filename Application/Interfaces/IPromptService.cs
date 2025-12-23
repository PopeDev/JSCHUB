using JSCHUB.Application.DTOs;

namespace JSCHUB.Application.Interfaces;

/// <summary>
/// Servicio para gestión de Prompts
/// </summary>
public interface IPromptService
{
    Task<PromptDto?> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<IEnumerable<PromptDto>> GetAllAsync(CancellationToken ct = default);
    Task<IEnumerable<PromptDto>> GetActivosAsync(CancellationToken ct = default);
    Task<IEnumerable<PromptDto>> SearchAsync(
        string? searchText = null,
        Guid? toolId = null,
        Guid? proyectoId = null,
        Guid? tagId = null,
        bool incluirInactivos = false,
        int skip = 0,
        int take = 50,
        CancellationToken ct = default);
    Task<PromptDto> CreateAsync(CreatePromptDto dto, CancellationToken ct = default);
    Task<PromptDto> UpdateAsync(Guid id, UpdatePromptDto dto, CancellationToken ct = default);
    Task ToggleActivoAsync(Guid id, CancellationToken ct = default);
    Task DeleteAsync(Guid id, CancellationToken ct = default);
}
