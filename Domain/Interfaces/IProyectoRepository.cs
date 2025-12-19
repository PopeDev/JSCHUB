using JSCHUB.Domain.Entities;
using JSCHUB.Domain.Enums;

namespace JSCHUB.Domain.Interfaces;

public interface IProyectoRepository
{
    Task<Proyecto?> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<Proyecto?> GetByIdWithEnlacesAsync(Guid id, CancellationToken ct = default);
    Task<Proyecto?> GetByIdWithRecursosAsync(Guid id, CancellationToken ct = default);
    Task<Proyecto?> GetByIdCompleteAsync(Guid id, CancellationToken ct = default);
    Task<Proyecto?> GetByNombreAsync(string nombre, CancellationToken ct = default);

    Task<IEnumerable<Proyecto>> SearchAsync(
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

    Task<Proyecto> AddAsync(Proyecto proyecto, CancellationToken ct = default);
    Task UpdateAsync(Proyecto proyecto, CancellationToken ct = default);
    Task<bool> ExisteNombreAsync(string nombre, Guid? excluirId = null, CancellationToken ct = default);
}
