using JSCHUB.Application.DTOs;

namespace JSCHUB.Application.Interfaces;

/// <summary>
/// Servicio para gestión de Usuarios
/// </summary>
public interface IUsuarioService
{
    Task<UsuarioDto?> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<UsuarioDto?> GetByNombreAsync(string nombre, CancellationToken ct = default);
    Task<IEnumerable<UsuarioDto>> GetAllAsync(CancellationToken ct = default);
    Task<IEnumerable<UsuarioDto>> GetActivosAsync(CancellationToken ct = default);
    Task<UsuarioDto> CreateAsync(CreateUsuarioDto dto, CancellationToken ct = default);
    Task<UsuarioDto> UpdateAsync(Guid id, UpdateUsuarioDto dto, CancellationToken ct = default);
    Task ToggleActivoAsync(Guid id, CancellationToken ct = default);
    Task DeleteAsync(Guid id, CancellationToken ct = default);
}
