using JSCHUB.Application.DTOs;
using JSCHUB.Domain.Enums;

namespace JSCHUB.Application.Interfaces;

public interface IProyectoService
{
    Task<ProyectoDto?> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<ProyectoDetalleDto?> GetDetalleAsync(Guid id, CancellationToken ct = default);

    Task<IEnumerable<ProyectoDto>> SearchAsync(
        string? searchText = null,
        EstadoProyecto? estado = null,
        string? etiqueta = null,
        bool incluirArchivados = false,
        int skip = 0,
        int take = 50,
        CancellationToken ct = default);

    Task<int> CountAsync(
        string? searchText = null,
        EstadoProyecto? estado = null,
        string? etiqueta = null,
        bool incluirArchivados = false,
        CancellationToken ct = default);

    Task<ProyectoDto> CreateAsync(CreateProyectoDto dto, string usuario, CancellationToken ct = default);
    Task<ProyectoDto> UpdateAsync(Guid id, UpdateProyectoDto dto, string usuario, CancellationToken ct = default);
    Task ArchivarAsync(Guid id, string usuario, CancellationToken ct = default);
    Task ReactivarAsync(Guid id, string usuario, CancellationToken ct = default);
    Task<bool> ExisteNombreAsync(string nombre, Guid? excluirId = null, CancellationToken ct = default);
}
