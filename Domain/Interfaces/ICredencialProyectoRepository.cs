using JSCHUB.Domain.Entities;

namespace JSCHUB.Domain.Interfaces;

public interface ICredencialProyectoRepository
{
    Task<CredencialProyecto?> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<CredencialProyecto?> GetByIdWithEnlaceAsync(Guid id, CancellationToken ct = default);
    Task<IEnumerable<CredencialProyecto>> GetByEnlaceIdAsync(Guid enlaceProyectoId, CancellationToken ct = default);
    Task<IEnumerable<CredencialProyecto>> GetByProyectoIdAsync(Guid proyectoId, CancellationToken ct = default);

    Task<IEnumerable<CredencialProyecto>> SearchAsync(
        Guid? proyectoId = null,
        Guid? enlaceProyectoId = null,
        string? searchText = null,
        bool? activa = null,
        CancellationToken ct = default);

    Task<CredencialProyecto> AddAsync(CredencialProyecto credencial, CancellationToken ct = default);
    Task UpdateAsync(CredencialProyecto credencial, CancellationToken ct = default);
    Task DeleteAsync(CredencialProyecto credencial, CancellationToken ct = default);
    Task<bool> ExisteNombreEnEnlaceAsync(Guid enlaceProyectoId, string nombre, Guid? excludeId = null, CancellationToken ct = default);
}
