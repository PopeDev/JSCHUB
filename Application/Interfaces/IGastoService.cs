using JSCHUB.Application.DTOs;
using JSCHUB.Domain.Enums;

namespace JSCHUB.Application.Interfaces;

/// <summary>
/// Servicio para gestión de Gastos
/// </summary>
public interface IGastoService
{
    Task<GastoDto?> GetByIdAsync(Guid id, CancellationToken ct = default);

    /// <summary>
    /// Busca gastos con filtros.
    /// </summary>
    /// <param name="usuarioActualId">Id del usuario actual (para filtrar por proyectos asignados)</param>
    /// <param name="proyectoId">Filtrar por proyecto específico (incluye Proyecto General automáticamente)</param>
    Task<IEnumerable<GastoDto>> SearchAsync(
        Guid usuarioActualId,
        string? searchText = null,
        Guid? pagadoPorId = null,
        DateOnly? fechaDesde = null,
        DateOnly? fechaHasta = null,
        decimal? importeMin = null,
        decimal? importeMax = null,
        EstadoGasto? estado = null,
        Guid? proyectoId = null,
        int skip = 0,
        int take = 50,
        CancellationToken ct = default);

    Task<int> CountAsync(
        Guid usuarioActualId,
        string? searchText = null,
        Guid? pagadoPorId = null,
        DateOnly? fechaDesde = null,
        DateOnly? fechaHasta = null,
        decimal? importeMin = null,
        decimal? importeMax = null,
        EstadoGasto? estado = null,
        Guid? proyectoId = null,
        CancellationToken ct = default);

    /// <summary>
    /// Crea un gasto. El usuario debe tener acceso a los proyectos especificados.
    /// Si no se especifican proyectos, se asocia al Proyecto General (si el usuario tiene acceso).
    /// </summary>
    /// <param name="usuarioActualId">Id del usuario que crea el gasto</param>
    Task<GastoDto> CreateAsync(Guid usuarioActualId, CreateGastoDto dto, CancellationToken ct = default);

    /// <summary>
    /// Actualiza un gasto. Requiere permisos según el rol del usuario.
    /// </summary>
    Task<GastoDto> UpdateAsync(Guid usuarioActualId, Guid id, UpdateGastoDto dto, CancellationToken ct = default);

    Task AnularAsync(Guid usuarioActualId, Guid id, CancellationToken ct = default);
    Task DeleteAsync(Guid usuarioActualId, Guid id, CancellationToken ct = default);
}
