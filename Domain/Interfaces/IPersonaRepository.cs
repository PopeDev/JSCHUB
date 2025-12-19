using JSCHUB.Domain.Entities;

namespace JSCHUB.Domain.Interfaces;

/// <summary>
/// Repositorio para gestión de Personas
/// </summary>
public interface IPersonaRepository
{
    Task<Persona?> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<IEnumerable<Persona>> GetAllAsync(CancellationToken ct = default);
    Task<IEnumerable<Persona>> GetActivasAsync(CancellationToken ct = default);
    Task<Persona> AddAsync(Persona persona, CancellationToken ct = default);
    Task UpdateAsync(Persona persona, CancellationToken ct = default);
    Task<bool> ExistsAsync(Guid id, CancellationToken ct = default);
}
