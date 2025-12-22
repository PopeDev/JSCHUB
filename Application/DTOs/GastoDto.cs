using JSCHUB.Domain.Enums;

namespace JSCHUB.Application.DTOs;

/// <summary>
/// DTO simplificado de proyecto para mostrar en gastos/recordatorios
/// </summary>
public record ProyectoSimpleDto(
    Guid Id,
    string Nombre,
    bool EsGeneral
);

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
    EstadoGasto Estado,
    List<ProyectoSimpleDto> Proyectos,
    bool EsGeneral // true si solo está asociado al Proyecto General
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
    TimeOnly HoraPago,
    List<Guid>? ProyectoIds // Si es null o vacío, se asocia al Proyecto General
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
    EstadoGasto Estado,
    List<Guid>? ProyectoIds // Si es null o vacío, se asocia al Proyecto General
);
