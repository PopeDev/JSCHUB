using JSCHUB.Application.DTOs;
using JSCHUB.Domain.Enums;

namespace JSCHUB.Application.Interfaces;

/// <summary>
/// Servicio para gestión de Gastos
/// </summary>
public interface IGastoService
{
    Task<GastoDto?> GetByIdAsync(Guid id, CancellationToken ct = default);
    
    Task<IEnumerable<GastoDto>> SearchAsync(
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
    
    Task<GastoDto> CreateAsync(CreateGastoDto dto, CancellationToken ct = default);
    Task<GastoDto> UpdateAsync(Guid id, UpdateGastoDto dto, CancellationToken ct = default);
    Task AnularAsync(Guid id, CancellationToken ct = default);
    Task DeleteAsync(Guid id, CancellationToken ct = default);
}
