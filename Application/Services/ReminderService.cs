using System.Text.Json;
using JSCHUB.Application.DTOs;
using JSCHUB.Application.Interfaces;
using JSCHUB.Domain.Entities;
using JSCHUB.Domain.Enums;
using JSCHUB.Domain.Interfaces;
using Microsoft.Extensions.Logging;

namespace JSCHUB.Application.Services;

public class ReminderService : IReminderService
{
    private readonly IReminderItemRepository _repository;
    private readonly IAlertRepository _alertRepository;
    private readonly IUsuarioProyectoRepository _usuarioProyectoRepository;
    private readonly IUsuarioRepository _usuarioRepository;
    private readonly IAuditService _auditService;
    private readonly ILogger<ReminderService> _logger;

    public ReminderService(
        IReminderItemRepository repository,
        IAlertRepository alertRepository,
        IUsuarioProyectoRepository usuarioProyectoRepository,
        IUsuarioRepository usuarioRepository,
        IAuditService auditService,
        ILogger<ReminderService> logger)
    {
        _repository = repository;
        _alertRepository = alertRepository;
        _usuarioProyectoRepository = usuarioProyectoRepository;
        _usuarioRepository = usuarioRepository;
        _auditService = auditService;
        _logger = logger;
    }

    public async Task<ReminderItemDto?> GetByIdAsync(Guid id, CancellationToken ct = default)
    {
        var item = await _repository.GetByIdWithRelacionesAsync(id, ct);
        return item == null ? null : MapToDto(item);
    }

    public async Task<IEnumerable<ReminderItemDto>> SearchAsync(
        Guid usuarioActualId,
        string? searchText = null,
        Category? category = null,
        ItemStatus? status = null,
        string? tag = null,
        Guid? asignadoAId = null,
        Guid? proyectoId = null,
        int skip = 0,
        int take = 50,
        CancellationToken ct = default)
    {
        var proyectosUsuario = await _usuarioProyectoRepository.GetProyectosByUsuarioAsync(usuarioActualId, ct);
        var proyectoIds = proyectosUsuario.Select(p => p.ProyectoId).ToList();

        if (proyectoId.HasValue && !proyectoIds.Contains(proyectoId.Value))
        {
            throw new UnauthorizedAccessException("No tiene acceso al proyecto especificado");
        }

        var items = await _repository.SearchAsync(
            searchText, category, status, tag, asignadoAId, proyectoId, proyectoIds, skip, take, ct);

        return items.Select(MapToDto);
    }

    public async Task<int> CountAsync(
        Guid usuarioActualId,
        string? searchText = null,
        Category? category = null,
        ItemStatus? status = null,
        string? tag = null,
        Guid? asignadoAId = null,
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
            searchText, category, status, tag, asignadoAId, proyectoId, proyectoIds, ct);
    }

    public async Task<ReminderItemDto> CreateAsync(Guid usuarioActualId, CreateReminderItemDto dto, CancellationToken ct = default)
    {
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
            throw new UnauthorizedAccessException("No tiene permisos para crear recordatorios en los proyectos especificados");

        // Validar usuario asignado (si se especifica)
        if (dto.AsignadoAId.HasValue)
        {
            var usuarioAsignado = await _usuarioRepository.GetByIdAsync(dto.AsignadoAId.Value, ct)
                ?? throw new KeyNotFoundException($"Usuario asignado {dto.AsignadoAId} no encontrado");

            if (!usuarioAsignado.Activo)
                throw new InvalidOperationException($"El usuario {usuarioAsignado.Nombre} no está activo");

            // Validar que el usuario asignado tiene acceso a todos los proyectos
            var asignadoTieneAcceso = await _usuarioProyectoRepository.TieneAccesoATodosAsync(dto.AsignadoAId.Value, proyectoIdsParaAsociar, ct);
            if (!asignadoTieneAcceso)
                throw new InvalidOperationException($"El usuario {usuarioAsignado.Nombre} no tiene acceso a todos los proyectos del recordatorio");
        }

        var item = new ReminderItem
        {
            Id = Guid.NewGuid(),
            Title = dto.Title,
            Description = dto.Description,
            Category = dto.Category,
            Tags = dto.Tags ?? [],
            AsignadoAId = dto.AsignadoAId,
            Status = ItemStatus.Active,
            ScheduleType = dto.ScheduleType,
            DueAt = dto.DueAt,
            RecurrenceFrequency = dto.RecurrenceFrequency,
            CustomIntervalDays = dto.CustomIntervalDays,
            Timezone = dto.Timezone ?? "Europe/Madrid",
            LeadTimeDays = dto.LeadTimeDays ?? [30],
            Metadata = dto.Metadata ?? new(),
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        if (item.ScheduleType == ScheduleType.OneTime)
        {
            item.NextOccurrenceAt = item.DueAt;
        }
        else
        {
            item.NextOccurrenceAt = item.CalculateNextOccurrence(DateTime.UtcNow);
        }

        await _repository.AddAsync(item, ct);
        await _repository.SetProyectosAsync(item.Id, proyectoIdsParaAsociar, ct);
        await _auditService.LogAsync("ReminderItem", item.Id, "Create", JsonSerializer.Serialize(dto), ct);

        _logger.LogInformation("Created ReminderItem {Id}: {Title} - Proyectos: {Proyectos}",
            item.Id, item.Title, string.Join(", ", proyectoIdsParaAsociar));

        var itemCompleto = await _repository.GetByIdWithRelacionesAsync(item.Id, ct);
        return MapToDto(itemCompleto!);
    }

    public async Task<ReminderItemDto> UpdateAsync(Guid usuarioActualId, Guid id, UpdateReminderItemDto dto, CancellationToken ct = default)
    {
        var item = await _repository.GetByIdWithRelacionesAsync(id, ct)
            ?? throw new KeyNotFoundException($"ReminderItem {id} not found");

        await ValidarPermisosEdicionAsync(usuarioActualId, item, ct);

        var nuevosProyectoIds = await DeterminarProyectosAsync(usuarioActualId, dto.ProyectoIds, ct);

        var tieneAcceso = await _usuarioProyectoRepository.TieneAccesoATodosAsync(usuarioActualId, nuevosProyectoIds, ct);
        if (!tieneAcceso)
            throw new UnauthorizedAccessException("No tiene acceso a todos los proyectos especificados");

        // Validar usuario asignado (si se especifica)
        if (dto.AsignadoAId.HasValue)
        {
            var usuarioAsignado = await _usuarioRepository.GetByIdAsync(dto.AsignadoAId.Value, ct)
                ?? throw new KeyNotFoundException($"Usuario asignado {dto.AsignadoAId} no encontrado");

            if (!usuarioAsignado.Activo)
                throw new InvalidOperationException($"El usuario {usuarioAsignado.Nombre} no está activo");

            var asignadoTieneAcceso = await _usuarioProyectoRepository.TieneAccesoATodosAsync(dto.AsignadoAId.Value, nuevosProyectoIds, ct);
            if (!asignadoTieneAcceso)
                throw new InvalidOperationException($"El usuario {usuarioAsignado.Nombre} no tiene acceso a todos los proyectos del recordatorio");
        }

        item.Title = dto.Title;
        item.Description = dto.Description;
        item.Category = dto.Category;
        item.Tags = dto.Tags ?? [];
        item.AsignadoAId = dto.AsignadoAId;
        item.Status = dto.Status;
        item.ScheduleType = dto.ScheduleType;
        item.DueAt = dto.DueAt;
        item.RecurrenceFrequency = dto.RecurrenceFrequency;
        item.CustomIntervalDays = dto.CustomIntervalDays;
        item.Timezone = dto.Timezone ?? "Europe/Madrid";
        item.LeadTimeDays = dto.LeadTimeDays ?? [30];
        item.Metadata = dto.Metadata ?? new();
        item.UpdatedAt = DateTime.UtcNow;

        if (item.ScheduleType == ScheduleType.OneTime)
        {
            item.NextOccurrenceAt = item.DueAt;
        }
        else
        {
            item.NextOccurrenceAt = item.CalculateNextOccurrence(DateTime.UtcNow);
        }

        await _repository.UpdateAsync(item, ct);
        await _repository.SetProyectosAsync(item.Id, nuevosProyectoIds, ct);
        await _auditService.LogAsync("ReminderItem", item.Id, "Update", JsonSerializer.Serialize(dto), ct);

        var itemCompleto = await _repository.GetByIdWithRelacionesAsync(item.Id, ct);
        return MapToDto(itemCompleto!);
    }

    public async Task DeleteAsync(Guid usuarioActualId, Guid id, CancellationToken ct = default)
    {
        var item = await _repository.GetByIdWithRelacionesAsync(id, ct)
            ?? throw new KeyNotFoundException($"ReminderItem {id} not found");

        await ValidarPermisosEdicionAsync(usuarioActualId, item, ct);

        item.Status = ItemStatus.Archived;
        item.UpdatedAt = DateTime.UtcNow;

        await _repository.UpdateAsync(item, ct);
        await _auditService.LogAsync("ReminderItem", id, "Archive", null, ct);
    }

    public async Task<ReminderItemDto> CompleteAsync(Guid usuarioActualId, Guid id, CancellationToken ct = default)
    {
        var item = await _repository.GetByIdWithRelacionesAsync(id, ct)
            ?? throw new KeyNotFoundException($"ReminderItem {id} not found");

        await ValidarPermisosEdicionAsync(usuarioActualId, item, ct);

        item.Complete();

        foreach (var alert in item.Alerts.Where(a => a.State is AlertState.Open or AlertState.Acknowledged))
        {
            alert.Resolve();
            await _alertRepository.UpdateAsync(alert, ct);
        }

        await _repository.UpdateAsync(item, ct);
        await _auditService.LogAsync("ReminderItem", item.Id, "Complete",
            $"{{\"nextOccurrence\": \"{item.NextOccurrenceAt}\"}}", ct);

        _logger.LogInformation("Completed ReminderItem {Id}, next occurrence: {Next}", id, item.NextOccurrenceAt);

        return MapToDto(item);
    }

    public async Task<ReminderItemDto> SnoozeAsync(Guid usuarioActualId, Guid id, DateTime snoozedUntil, CancellationToken ct = default)
    {
        var item = await _repository.GetByIdWithRelacionesAsync(id, ct)
            ?? throw new KeyNotFoundException($"ReminderItem {id} not found");

        await ValidarPermisosEdicionAsync(usuarioActualId, item, ct);

        foreach (var alert in item.Alerts.Where(a => a.State is AlertState.Open or AlertState.Acknowledged))
        {
            alert.Snooze(snoozedUntil);
            await _alertRepository.UpdateAsync(alert, ct);
        }

        await _auditService.LogAsync("ReminderItem", item.Id, "Snooze",
            $"{{\"snoozedUntil\": \"{snoozedUntil}\"}}", ct);

        return MapToDto(item);
    }

    public async Task<ReminderItemDto> PauseAsync(Guid usuarioActualId, Guid id, CancellationToken ct = default)
    {
        var item = await _repository.GetByIdWithRelacionesAsync(id, ct)
            ?? throw new KeyNotFoundException($"ReminderItem {id} not found");

        await ValidarPermisosEdicionAsync(usuarioActualId, item, ct);

        item.Status = ItemStatus.Paused;
        item.UpdatedAt = DateTime.UtcNow;

        await _repository.UpdateAsync(item, ct);
        await _auditService.LogAsync("ReminderItem", item.Id, "Pause", null, ct);

        return MapToDto(item);
    }

    public async Task<ReminderItemDto> ResumeAsync(Guid usuarioActualId, Guid id, CancellationToken ct = default)
    {
        var item = await _repository.GetByIdWithRelacionesAsync(id, ct)
            ?? throw new KeyNotFoundException($"ReminderItem {id} not found");

        await ValidarPermisosEdicionAsync(usuarioActualId, item, ct);

        item.Status = ItemStatus.Active;
        item.NextOccurrenceAt = item.CalculateNextOccurrence(DateTime.UtcNow);
        item.UpdatedAt = DateTime.UtcNow;

        await _repository.UpdateAsync(item, ct);
        await _auditService.LogAsync("ReminderItem", item.Id, "Resume", null, ct);

        return MapToDto(item);
    }

    private async Task<List<Guid>> DeterminarProyectosAsync(Guid usuarioActualId, List<Guid>? proyectoIds, CancellationToken ct)
    {
        if (proyectoIds == null || !proyectoIds.Any())
        {
            var proyectoGeneralId = await _repository.GetProyectoGeneralIdAsync(ct)
                ?? throw new InvalidOperationException("No existe el Proyecto General en el sistema");

            var tieneAccesoGeneral = await _usuarioProyectoRepository.TieneAccesoAsync(usuarioActualId, proyectoGeneralId, ct);
            if (!tieneAccesoGeneral)
                throw new UnauthorizedAccessException("No tiene acceso al Proyecto General. Debe especificar proyectos.");

            return [proyectoGeneralId];
        }

        var proyectoGeneralIdCheck = await _repository.GetProyectoGeneralIdAsync(ct);
        if (proyectoGeneralIdCheck.HasValue && proyectoIds.Contains(proyectoGeneralIdCheck.Value) && proyectoIds.Count > 1)
        {
            throw new InvalidOperationException("Si se selecciona el Proyecto General, no se pueden seleccionar otros proyectos");
        }

        return proyectoIds;
    }

    private async Task ValidarPermisosEdicionAsync(Guid usuarioActualId, ReminderItem item, CancellationToken ct)
    {
        var esPropio = item.AsignadoAId == usuarioActualId;

        foreach (var rp in item.ReminderItemsProyecto)
        {
            var rol = await _usuarioProyectoRepository.GetRolAsync(usuarioActualId, rp.ProyectoId, ct);
            if (rol == null) continue;

            if (rol == RolProyecto.Admin) return;
            if (rol == RolProyecto.Miembro && esPropio) return;
        }

        throw new UnauthorizedAccessException(esPropio
            ? "No tiene permisos para editar este recordatorio"
            : "Solo puede editar sus propios recordatorios o necesita ser Admin del proyecto");
    }

    private static ReminderItemDto MapToDto(ReminderItem item)
    {
        var proyectos = item.ReminderItemsProyecto?.Select(rp => new ProyectoSimpleDto(
            rp.Proyecto.Id,
            rp.Proyecto.Nombre,
            rp.Proyecto.EsGeneral
        )).ToList() ?? [];

        var esGeneral = proyectos.Count == 1 && proyectos[0].EsGeneral;

        return new ReminderItemDto(
            item.Id,
            item.Title,
            item.Description,
            item.Category,
            item.Tags,
            item.AsignadoAId,
            item.AsignadoA?.Nombre,
            item.Status,
            item.ScheduleType,
            item.DueAt,
            item.RecurrenceFrequency,
            item.CustomIntervalDays,
            item.Timezone,
            item.LeadTimeDays,
            item.NextOccurrenceAt,
            item.LastOccurrenceAt,
            item.Metadata,
            item.CreatedAt,
            item.UpdatedAt,
            item.Alerts?.Count(a => a.State is AlertState.Open or AlertState.Acknowledged) ?? 0,
            proyectos,
            esGeneral
        );
    }
}
