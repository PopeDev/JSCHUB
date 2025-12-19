using JSCHUB.Application.DTOs;
using JSCHUB.Application.Interfaces;
using JSCHUB.Domain.Entities;
using JSCHUB.Domain.Enums;
using JSCHUB.Domain.Interfaces;
using Microsoft.Extensions.Logging;

namespace JSCHUB.Application.Services;

public class GastoService : IGastoService
{
    private readonly IGastoRepository _repository;
    private readonly IPersonaRepository _personaRepository;
    private readonly ILogger<GastoService> _logger;

    public GastoService(
        IGastoRepository repository,
        IPersonaRepository personaRepository,
        ILogger<GastoService> logger)
    {
        _repository = repository;
        _personaRepository = personaRepository;
        _logger = logger;
    }

    public async Task<GastoDto?> GetByIdAsync(Guid id, CancellationToken ct = default)
    {
        var gasto = await _repository.GetByIdWithPersonaAsync(id, ct);
        return gasto == null ? null : MapToDto(gasto);
    }

    public async Task<IEnumerable<GastoDto>> SearchAsync(
        string? searchText = null,
        Guid? pagadoPorId = null,
        DateOnly? fechaDesde = null,
        DateOnly? fechaHasta = null,
        decimal? importeMin = null,
        decimal? importeMax = null,
        EstadoGasto? estado = null,
        int skip = 0,
        int take = 50,
        CancellationToken ct = default)
    {
        var gastos = await _repository.SearchAsync(
            searchText, pagadoPorId, fechaDesde, fechaHasta,
            importeMin, importeMax, estado, skip, take, ct);
        return gastos.Select(MapToDto);
    }

    public async Task<int> CountAsync(
        string? searchText = null,
        Guid? pagadoPorId = null,
        DateOnly? fechaDesde = null,
        DateOnly? fechaHasta = null,
        decimal? importeMin = null,
        decimal? importeMax = null,
        EstadoGasto? estado = null,
        CancellationToken ct = default)
    {
        return await _repository.CountAsync(
            searchText, pagadoPorId, fechaDesde, fechaHasta,
            importeMin, importeMax, estado, ct);
    }

    public async Task<GastoDto> CreateAsync(CreateGastoDto dto, CancellationToken ct = default)
    {
        // Validar importe
        if (dto.Importe <= 0)
            throw new ArgumentException("El importe debe ser mayor que 0");

        // Validar que la persona existe y está activa
        var persona = await _personaRepository.GetByIdAsync(dto.PagadoPorId, ct)
            ?? throw new KeyNotFoundException($"Persona {dto.PagadoPorId} no encontrada");
        
        if (!persona.Activo)
            throw new InvalidOperationException($"La persona {persona.Nombre} no está activa");

        var gasto = new Gasto
        {
            Id = Guid.NewGuid(),
            Concepto = dto.Concepto,
            Notas = dto.Notas,
            Importe = dto.Importe,
            Moneda = dto.Moneda ?? "EUR",
            PagadoPorId = dto.PagadoPorId,
            FechaPago = dto.FechaPago,
            HoraPago = dto.HoraPago,
            Estado = EstadoGasto.Registrado
        };

        await _repository.AddAsync(gasto, ct);
        _logger.LogInformation("Gasto creado: {Id} - {Concepto} - {Importe}{Moneda}", 
            gasto.Id, gasto.Concepto, gasto.Importe, gasto.Moneda);

        // Cargar relación para el DTO
        gasto.PagadoPor = persona;
        return MapToDto(gasto);
    }

    public async Task<GastoDto> UpdateAsync(Guid id, UpdateGastoDto dto, CancellationToken ct = default)
    {
        // Validar importe
        if (dto.Importe <= 0)
            throw new ArgumentException("El importe debe ser mayor que 0");

        var gasto = await _repository.GetByIdAsync(id, ct)
            ?? throw new KeyNotFoundException($"Gasto {id} no encontrado");

        // Validar que la persona existe y está activa
        var persona = await _personaRepository.GetByIdAsync(dto.PagadoPorId, ct)
            ?? throw new KeyNotFoundException($"Persona {dto.PagadoPorId} no encontrada");
        
        if (!persona.Activo)
            throw new InvalidOperationException($"La persona {persona.Nombre} no está activa");

        gasto.Concepto = dto.Concepto;
        gasto.Notas = dto.Notas;
        gasto.Importe = dto.Importe;
        gasto.Moneda = dto.Moneda ?? "EUR";
        gasto.PagadoPorId = dto.PagadoPorId;
        gasto.FechaPago = dto.FechaPago;
        gasto.HoraPago = dto.HoraPago;
        gasto.Estado = dto.Estado;

        await _repository.UpdateAsync(gasto, ct);

        gasto.PagadoPor = persona;
        return MapToDto(gasto);
    }

    public async Task AnularAsync(Guid id, CancellationToken ct = default)
    {
        var gasto = await _repository.GetByIdAsync(id, ct)
            ?? throw new KeyNotFoundException($"Gasto {id} no encontrado");

        gasto.Estado = EstadoGasto.Anulado;
        await _repository.UpdateAsync(gasto, ct);
        
        _logger.LogInformation("Gasto anulado: {Id}", id);
    }

    private static GastoDto MapToDto(Gasto gasto) => new(
        gasto.Id,
        gasto.Concepto,
        gasto.Notas,
        gasto.Importe,
        gasto.Moneda,
        gasto.PagadoPorId,
        gasto.PagadoPor?.Nombre ?? "Desconocido",
        gasto.FechaPago,
        gasto.HoraPago,
        gasto.Estado
    );
}
