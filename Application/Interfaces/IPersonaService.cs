using JSCHUB.Application.DTOs;

namespace JSCHUB.Application.Interfaces;

/// <summary>
/// Servicio para gestión de Personas
/// </summary>
public interface IPersonaService
{
    Task<PersonaDto?> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<IEnumerable<PersonaDto>> GetAllAsync(CancellationToken ct = default);
    Task<IEnumerable<PersonaDto>> GetActivasAsync(CancellationToken ct = default);
    Task<PersonaDto> CreateAsync(CreatePersonaDto dto, CancellationToken ct = default);
    Task<PersonaDto> UpdateAsync(Guid id, UpdatePersonaDto dto, CancellationToken ct = default);
    Task ToggleActivoAsync(Guid id, CancellationToken ct = default);
}
