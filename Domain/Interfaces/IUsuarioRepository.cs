using JSCHUB.Domain.Entities;

namespace JSCHUB.Domain.Interfaces;

/// <summary>
/// Repositorio para gestión de Usuarios
/// </summary>
public interface IUsuarioRepository
{
    Task<Usuario?> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<Usuario?> GetByNombreAsync(string nombre, CancellationToken ct = default);
    Task<IEnumerable<Usuario>> GetAllAsync(CancellationToken ct = default);
    Task<IEnumerable<Usuario>> GetActivosAsync(CancellationToken ct = default);
    Task<Usuario> AddAsync(Usuario usuario, CancellationToken ct = default);
    Task UpdateAsync(Usuario usuario, CancellationToken ct = default);
    Task DeleteAsync(Usuario usuario, CancellationToken ct = default);
    Task<bool> ExistsAsync(Guid id, CancellationToken ct = default);
}
