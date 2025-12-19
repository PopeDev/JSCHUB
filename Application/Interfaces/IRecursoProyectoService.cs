using JSCHUB.Application.DTOs;
using JSCHUB.Domain.Enums;

namespace JSCHUB.Application.Interfaces;

public interface IRecursoProyectoService
{
    Task<RecursoProyectoDto?> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<IEnumerable<RecursoProyectoDto>> GetByProyectoIdAsync(Guid proyectoId, CancellationToken ct = default);

    Task<IEnumerable<RecursoProyectoDto>> SearchAsync(
        Guid proyectoId,
        string? searchText = null,
        TipoRecurso? tipo = null,
        string? etiqueta = null,
        CancellationToken ct = default);

    Task<RecursoProyectoDto> CreateAsync(CreateRecursoProyectoDto dto, string usuario, CancellationToken ct = default);
    Task<RecursoProyectoDto> UpdateAsync(Guid id, UpdateRecursoProyectoDto dto, string usuario, CancellationToken ct = default);
    Task DeleteAsync(Guid id, CancellationToken ct = default);
}
