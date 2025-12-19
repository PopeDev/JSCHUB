using JSCHUB.Domain.Enums;

namespace JSCHUB.Application.DTOs;

/// <summary>
/// DTO para lectura de Gasto
/// </summary>
public record GastoDto(
    Guid Id,
    string Concepto,
    string? Notas,
    decimal Importe,
    string Moneda,
    Guid PagadoPorId,
    string PagadoPorNombre,
    DateOnly FechaPago,
    TimeOnly HoraPago,
    EstadoGasto Estado
);

/// <summary>
/// DTO para creación de Gasto
/// </summary>
public record CreateGastoDto(
    string Concepto,
    string? Notas,
    decimal Importe,
    string? Moneda,
    Guid PagadoPorId,
    DateOnly FechaPago,
    TimeOnly HoraPago
);

/// <summary>
/// DTO para actualización de Gasto
/// </summary>
public record UpdateGastoDto(
    string Concepto,
    string? Notas,
    decimal Importe,
    string? Moneda,
    Guid PagadoPorId,
    DateOnly FechaPago,
    TimeOnly HoraPago,
    EstadoGasto Estado
);
