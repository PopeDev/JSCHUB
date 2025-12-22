using JSCHUB.Application.DTOs;

namespace JSCHUB.Application.Interfaces;

/// <summary>
/// Servicio para gestión de Tools (herramientas de IA)
/// </summary>
public interface IToolService
{
    Task<ToolDto?> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<IEnumerable<ToolDto>> GetAllAsync(CancellationToken ct = default);
    Task<IEnumerable<ToolDto>> GetActivosAsync(CancellationToken ct = default);
    Task<ToolDto> CreateAsync(CreateToolDto dto, CancellationToken ct = default);
    Task<ToolDto> UpdateAsync(Guid id, UpdateToolDto dto, CancellationToken ct = default);
    Task ToggleActivoAsync(Guid id, CancellationToken ct = default);
    Task DeleteAsync(Guid id, CancellationToken ct = default);
}
