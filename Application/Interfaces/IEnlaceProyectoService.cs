using JSCHUB.Application.DTOs;
using JSCHUB.Domain.Enums;

namespace JSCHUB.Application.Interfaces;

public interface IEnlaceProyectoService
{
    Task<EnlaceProyectoDto?> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<IEnumerable<EnlaceProyectoDto>> GetByProyectoIdAsync(Guid proyectoId, CancellationToken ct = default);

    Task<IEnumerable<EnlaceProyectoDto>> SearchAsync(
        Guid proyectoId,
        string? searchText = null,
        TipoEnlace? tipo = null,
        CancellationToken ct = default);

    Task<EnlaceProyectoDto> CreateAsync(CreateEnlaceProyectoDto dto, string usuario, CancellationToken ct = default);
    Task<EnlaceProyectoDto> UpdateAsync(Guid id, UpdateEnlaceProyectoDto dto, string usuario, CancellationToken ct = default);
    Task DeleteAsync(Guid id, CancellationToken ct = default);
}
