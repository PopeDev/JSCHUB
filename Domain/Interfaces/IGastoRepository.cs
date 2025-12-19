using JSCHUB.Domain.Entities;
using JSCHUB.Domain.Enums;

namespace JSCHUB.Domain.Interfaces;

/// <summary>
/// Repositorio para gestión de Gastos
/// </summary>
public interface IGastoRepository
{
    Task<Gasto?> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<Gasto?> GetByIdWithPersonaAsync(Guid id, CancellationToken ct = default);
    
    Task<IEnumerable<Gasto>> SearchAsync(
        string? searchText = null,
        Guid? pagadoPorId = null,
        DateOnly? fechaDesde = null,
        DateOnly? fechaHasta = null,
        decimal? importeMin = null,
        decimal? importeMax = null,
        EstadoGasto? estado = null,
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
        CancellationToken ct = default);
    
    Task<Gasto> AddAsync(Gasto gasto, CancellationToken ct = default);
    Task UpdateAsync(Gasto gasto, CancellationToken ct = default);
}
