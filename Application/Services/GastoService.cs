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
    private readonly IUsuarioRepository _usuarioRepository;
    private readonly IUsuarioProyectoRepository _usuarioProyectoRepository;
    private readonly ILogger<GastoService> _logger;

    public GastoService(
        IGastoRepository repository,
        IUsuarioRepository usuarioRepository,
        IUsuarioProyectoRepository usuarioProyectoRepository,
        ILogger<GastoService> logger)
    {
        _repository = repository;
        _usuarioRepository = usuarioRepository;
        _usuarioProyectoRepository = usuarioProyectoRepository;
        _logger = logger;
    }

    public async Task<GastoDto?> GetByIdAsync(Guid id, CancellationToken ct = default)
    {
        var gasto = await _repository.GetByIdWithRelacionesAsync(id, ct);
        return gasto == null ? null : MapToDto(gasto);
    }

    public async Task<IEnumerable<GastoDto>> SearchAsync(
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
        CancellationToken ct = default)
    {
        // Obtener proyectos a los que el usuario tiene acceso
        var proyectosUsuario = await _usuarioProyectoRepository.GetProyectosByUsuarioAsync(usuarioActualId, ct);
        var proyectoIds = proyectosUsuario.Select(p => p.ProyectoId).ToList();

        // Validar que tiene acceso al proyecto filtrado
        if (proyectoId.HasValue && !proyectoIds.Contains(proyectoId.Value))
        {
            throw new UnauthorizedAccessException("No tiene acceso al proyecto especificado");
        }

        var gastos = await _repository.SearchAsync(
            searchText, pagadoPorId, fechaDesde, fechaHasta,
            importeMin, importeMax, estado, proyectoId, proyectoIds, skip, take, ct);

        return gastos.Select(MapToDto);
    }

    public async Task<int> CountAsync(
        Guid usuarioActualId,
        string? searchText = null,
        Guid? pagadoPorId = null,
        DateOnly? fechaDesde = null,
        DateOnly? fechaHasta = null,
        decimal? importeMin = null,
        decimal? importeMax = null,
        EstadoGasto? estado = null,
        Guid? proyectoId = null,
        CancellationToken ct = default)
    {
        var proyectosUsuario = await _usuarioProyectoRepository.GetProyectosByUsuarioAsync(usuarioActualId, ct);
        var proyectoIds = proyectosUsuario.Select(p => p.ProyectoId).ToList();

        if (proyectoId.HasValue && !proyectoIds.Contains(proyectoId.Value))
        {
            throw new UnauthorizedAccessException("No tiene acceso al proyecto especificado");
        }

        return await _repository.CountAsync(
            searchText, pagadoPorId, fechaDesde, fechaHasta,
            importeMin, importeMax, estado, proyectoId, proyectoIds, ct);
    }

    public async Task<GastoDto> CreateAsync(Guid usuarioActualId, CreateGastoDto dto, CancellationToken ct = default)
    {
        // Validar que el usuario pagador existe y está activo
        var usuarioPagador = await _usuarioRepository.GetByIdAsync(dto.PagadoPorId, ct)
            ?? throw new KeyNotFoundException($"Usuario {dto.PagadoPorId} no encontrado");

        if (!usuarioPagador.Activo)
            throw new InvalidOperationException($"El usuario {usuarioPagador.Nombre} no está activo");

        // Determinar proyectos a asociar
        var proyectoIdsParaAsociar = await DeterminarProyectosAsync(usuarioActualId, dto.ProyectoIds, ct);

        // Validar que el usuario actual tiene acceso a todos los proyectos
        var tieneAcceso = await _usuarioProyectoRepository.TieneAccesoATodosAsync(usuarioActualId, proyectoIdsParaAsociar, ct);
        if (!tieneAcceso)
            throw new UnauthorizedAccessException("No tiene acceso a todos los proyectos especificados");

        // Validar permisos de creación en al menos un proyecto
        var puedeCrear = false;
        foreach (var pId in proyectoIdsParaAsociar)
        {
            var rol = await _usuarioProyectoRepository.GetRolAsync(usuarioActualId, pId, ct);
            if (rol != null && rol != RolProyecto.Viewer)
            {
                puedeCrear = true;
                break;
            }
        }

        if (!puedeCrear)
            throw new UnauthorizedAccessException("No tiene permisos para crear gastos en los proyectos especificados");

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
            Estado = EstadoGasto.Previsto
        };

        await _repository.AddAsync(gasto, ct);

        // Asociar a proyectos
        await _repository.SetProyectosAsync(gasto.Id, proyectoIdsParaAsociar, ct);

        _logger.LogInformation("Gasto creado: {Id} - {Concepto} - {Importe}{Moneda} - Proyectos: {Proyectos}",
            gasto.Id, gasto.Concepto, gasto.Importe, gasto.Moneda, string.Join(", ", proyectoIdsParaAsociar));

        // Recargar para obtener relaciones
        var gastoCompleto = await _repository.GetByIdWithRelacionesAsync(gasto.Id, ct);
        return MapToDto(gastoCompleto!);
    }

    public async Task<GastoDto> UpdateAsync(Guid usuarioActualId, Guid id, UpdateGastoDto dto, CancellationToken ct = default)
    {
        var gasto = await _repository.GetByIdWithRelacionesAsync(id, ct)
            ?? throw new KeyNotFoundException($"Gasto {id} no encontrado");

        // Validar permisos
        await ValidarPermisosEdicionAsync(usuarioActualId, gasto, ct);

        // Validar que el usuario pagador existe y está activo
        var usuarioPagador = await _usuarioRepository.GetByIdAsync(dto.PagadoPorId, ct)
            ?? throw new KeyNotFoundException($"Usuario {dto.PagadoPorId} no encontrado");

        if (!usuarioPagador.Activo)
            throw new InvalidOperationException($"El usuario {usuarioPagador.Nombre} no está activo");

        // Determinar nuevos proyectos
        var nuevosProyectoIds = await DeterminarProyectosAsync(usuarioActualId, dto.ProyectoIds, ct);

        // Validar acceso a nuevos proyectos
        var tieneAcceso = await _usuarioProyectoRepository.TieneAccesoATodosAsync(usuarioActualId, nuevosProyectoIds, ct);
        if (!tieneAcceso)
            throw new UnauthorizedAccessException("No tiene acceso a todos los proyectos especificados");

        gasto.Concepto = dto.Concepto;
        gasto.Notas = dto.Notas;
        gasto.Importe = dto.Importe;
        gasto.Moneda = dto.Moneda ?? "EUR";
        gasto.PagadoPorId = dto.PagadoPorId;
        gasto.FechaPago = dto.FechaPago;
        gasto.HoraPago = dto.HoraPago;
        gasto.Estado = dto.Estado;

        await _repository.UpdateAsync(gasto, ct);
        await _repository.SetProyectosAsync(gasto.Id, nuevosProyectoIds, ct);

        var gastoCompleto = await _repository.GetByIdWithRelacionesAsync(gasto.Id, ct);
        return MapToDto(gastoCompleto!);
    }

    public async Task AnularAsync(Guid usuarioActualId, Guid id, CancellationToken ct = default)
    {
        var gasto = await _repository.GetByIdWithRelacionesAsync(id, ct)
            ?? throw new KeyNotFoundException($"Gasto {id} no encontrado");

        await ValidarPermisosEdicionAsync(usuarioActualId, gasto, ct);

        gasto.Estado = EstadoGasto.Anulado;
        await _repository.UpdateAsync(gasto, ct);

        _logger.LogInformation("Gasto anulado: {Id} por usuario {UsuarioId}", id, usuarioActualId);
    }

    public async Task DeleteAsync(Guid usuarioActualId, Guid id, CancellationToken ct = default)
    {
        var gasto = await _repository.GetByIdWithRelacionesAsync(id, ct)
            ?? throw new KeyNotFoundException($"Gasto {id} no encontrado");

        await ValidarPermisosEdicionAsync(usuarioActualId, gasto, ct);

        await _repository.DeleteAsync(gasto, ct);
        _logger.LogInformation("Gasto eliminado: {Id} por usuario {UsuarioId}", id, usuarioActualId);
    }

    private async Task<List<Guid>> DeterminarProyectosAsync(Guid usuarioActualId, List<Guid>? proyectoIds, CancellationToken ct)
    {
        // Si no se especifican proyectos, usar el Proyecto General
        if (proyectoIds == null || !proyectoIds.Any())
        {
            var proyectoGeneralId = await _repository.GetProyectoGeneralIdAsync(ct)
                ?? throw new InvalidOperationException("No existe el Proyecto General en el sistema");

            // Verificar que el usuario tiene acceso al Proyecto General
            var tieneAccesoGeneral = await _usuarioProyectoRepository.TieneAccesoAsync(usuarioActualId, proyectoGeneralId, ct);
            if (!tieneAccesoGeneral)
                throw new UnauthorizedAccessException("No tiene acceso al Proyecto General. Debe especificar proyectos.");

            return [proyectoGeneralId];
        }

        // Si hay proyectos, verificar que no mezclan General con otros
        var proyectoGeneralIdCheck = await _repository.GetProyectoGeneralIdAsync(ct);
        if (proyectoGeneralIdCheck.HasValue && proyectoIds.Contains(proyectoGeneralIdCheck.Value) && proyectoIds.Count > 1)
        {
            throw new InvalidOperationException("Si se selecciona el Proyecto General, no se pueden seleccionar otros proyectos");
        }

        return proyectoIds;
    }

    private async Task ValidarPermisosEdicionAsync(Guid usuarioActualId, Gasto gasto, CancellationToken ct)
    {
        var esPropio = gasto.PagadoPorId == usuarioActualId;

        // Verificar permisos en al menos uno de los proyectos del gasto
        foreach (var gp in gasto.GastosProyecto)
        {
            var rol = await _usuarioProyectoRepository.GetRolAsync(usuarioActualId, gp.ProyectoId, ct);
            if (rol == null) continue;

            // Admin puede editar/eliminar cualquier gasto
            if (rol == RolProyecto.Admin) return;

            // Miembro puede editar/eliminar sus propios gastos
            if (rol == RolProyecto.Miembro && esPropio) return;
        }

        throw new UnauthorizedAccessException(esPropio
            ? "No tiene permisos para editar este gasto"
            : "Solo puede editar sus propios gastos o necesita ser Admin del proyecto");
    }

    private static GastoDto MapToDto(Gasto gasto)
    {
        var proyectos = gasto.GastosProyecto?.Select(gp => new ProyectoSimpleDto(
            gp.Proyecto.Id,
            gp.Proyecto.Nombre,
            gp.Proyecto.EsGeneral
        )).ToList() ?? [];

        var esGeneral = proyectos.Count == 1 && proyectos[0].EsGeneral;

        return new GastoDto(
            gasto.Id,
            gasto.Concepto,
            gasto.Notas,
            gasto.Importe,
            gasto.Moneda,
            gasto.PagadoPorId,
            gasto.PagadoPor?.Nombre ?? "Desconocido",
            gasto.FechaPago,
            gasto.HoraPago,
            gasto.Estado,
            proyectos,
            esGeneral
        );
    }
}
