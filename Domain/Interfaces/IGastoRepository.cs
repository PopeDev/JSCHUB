using JSCHUB.Domain.Entities;
using JSCHUB.Domain.Enums;

namespace JSCHUB.Domain.Interfaces;

/// <summary>
/// Repositorio para gestión de Gastos
/// </summary>
public interface IGastoRepository
{
    Task<Gasto?> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<Gasto?> GetByIdWithRelacionesAsync(Guid id, CancellationToken ct = default);

    /// <summary>
    /// Busca gastos con filtros. Si se especifica proyectoId, incluye gastos del proyecto Y del Proyecto General.
    /// Si proyectoIds está vacío/null, retorna todos los gastos de los proyectos especificados en usuarioProyectoIds.
    /// </summary>
    Task<IEnumerable<Gasto>> SearchAsync(
        string? searchText = null,
        Guid? pagadoPorId = null,
        DateOnly? fechaDesde = null,
        DateOnly? fechaHasta = null,
        decimal? importeMin = null,
        decimal? importeMax = null,
        EstadoGasto? estado = null,
        Guid? proyectoId = null, // Filtrar por proyecto específico (incluye Proyecto General automáticamente)
        IEnumerable<Guid>? usuarioProyectoIds = null, // Proyectos a los que el usuario tiene acceso
        int skip = 0,
        int take = 50,
        CancellationToken ct = default);

    Task<int> CountAsync(
        string? searchText = null,
        Guid? pagadoPorId = null,
        DateOnly? fechaDesde = null,
        DateOnly? fechaHasta = null,
        decimal? importeMin = null,
        decimal? importeMax = null,
        EstadoGasto? estado = null,
        Guid? proyectoId = null,
        IEnumerable<Guid>? usuarioProyectoIds = null,
        CancellationToken ct = default);

    Task<Gasto> AddAsync(Gasto gasto, CancellationToken ct = default);
    Task UpdateAsync(Gasto gasto, CancellationToken ct = default);
    Task DeleteAsync(Gasto gasto, CancellationToken ct = default);

    /// <summary>
    /// Asocia un gasto a proyectos (reemplaza asociaciones existentes)
    /// </summary>
    Task SetProyectosAsync(Guid gastoId, IEnumerable<Guid> proyectoIds, CancellationToken ct = default);

    /// <summary>
    /// Obtiene el Id del Proyecto General
    /// </summary>
    Task<Guid?> GetProyectoGeneralIdAsync(CancellationToken ct = default);
}
