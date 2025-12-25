using JSCHUB.Application.DTOs;

namespace JSCHUB.Application.Interfaces;

public interface ICredencialProyectoService
{
    /// <summary>
    /// Obtiene una credencial por ID (sin contraseña)
    /// </summary>
    Task<CredencialProyectoDto?> GetByIdAsync(Guid id, CancellationToken ct = default);

    /// <summary>
    /// Obtiene una credencial con la contraseña descifrada
    /// </summary>
    Task<CredencialProyectoConPasswordDto?> GetByIdWithPasswordAsync(Guid id, string usuario, CancellationToken ct = default);

    /// <summary>
    /// Obtiene todas las credenciales de un enlace (sin contraseñas)
    /// </summary>
    Task<IEnumerable<CredencialProyectoDto>> GetByEnlaceIdAsync(Guid enlaceProyectoId, CancellationToken ct = default);

    /// <summary>
    /// Obtiene todas las credenciales de un proyecto (sin contraseñas)
    /// </summary>
    Task<IEnumerable<CredencialProyectoDto>> GetByProyectoIdAsync(Guid proyectoId, CancellationToken ct = default);

    /// <summary>
    /// Busca credenciales con filtros
    /// </summary>
    Task<IEnumerable<CredencialProyectoDto>> SearchAsync(
        Guid? proyectoId = null,
        Guid? enlaceProyectoId = null,
        string? searchText = null,
        bool? activa = null,
        CancellationToken ct = default);

    /// <summary>
    /// Crea una nueva credencial con la contraseña cifrada
    /// </summary>
    Task<CredencialProyectoDto> CreateAsync(CreateCredencialProyectoDto dto, string usuario, CancellationToken ct = default);

    /// <summary>
    /// Actualiza una credencial existente
    /// </summary>
    Task<CredencialProyectoDto> UpdateAsync(Guid id, UpdateCredencialProyectoDto dto, string usuario, CancellationToken ct = default);

    /// <summary>
    /// Elimina una credencial
    /// </summary>
    Task DeleteAsync(Guid id, CancellationToken ct = default);

    /// <summary>
    /// Copia la contraseña al portapapeles (registra acceso)
    /// </summary>
    Task<string> GetPasswordAsync(Guid id, string usuario, CancellationToken ct = default);
}
