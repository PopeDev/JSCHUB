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
    private readonly IAuditService _auditService;
    private readonly ILogger<ReminderService> _logger;

    public ReminderService(
        IReminderItemRepository repository,
        IAlertRepository alertRepository,
        IAuditService auditService,
        ILogger<ReminderService> logger)
    {
        _repository = repository;
        _alertRepository = alertRepository;
        _auditService = auditService;
        _logger = logger;
    }

    public async Task<ReminderItemDto?> GetByIdAsync(Guid id, CancellationToken ct = default)
    {
        var item = await _repository.GetByIdWithAlertsAsync(id, ct);
        return item == null ? null : MapToDto(item);
    }

    public async Task<IEnumerable<ReminderItemDto>> SearchAsync(
        string? searchText = null,
        Category? category = null,
        ItemStatus? status = null,
        string? tag = null,
        string? assignee = null,
        int skip = 0,
        int take = 50,
        CancellationToken ct = default)
    {
        var items = await _repository.SearchAsync(searchText, category, status, tag, assignee, skip, take, ct);
        return items.Select(MapToDto);
    }

    public async Task<int> CountAsync(
        string? searchText = null,
        Category? category = null,
        ItemStatus? status = null,
        string? tag = null,
        string? assignee = null,
        CancellationToken ct = default)
    {
        return await _repository.CountAsync(searchText, category, status, tag, assignee, ct);
    }

    public async Task<ReminderItemDto> CreateAsync(CreateReminderItemDto dto, CancellationToken ct = default)
    {
        var item = new ReminderItem
        {
            Id = Guid.NewGuid(),
            Title = dto.Title,
            Description = dto.Description,
            Category = dto.Category,
            Tags = dto.Tags ?? [],
            Assignee = dto.Assignee,
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

        // Calcular próxima ocurrencia
        if (item.ScheduleType == ScheduleType.OneTime)
        {
            item.NextOccurrenceAt = item.DueAt;
        }
        else
        {
            item.NextOccurrenceAt = item.CalculateNextOccurrence(DateTime.UtcNow);
        }

        await _repository.AddAsync(item, ct);
        await _auditService.LogAsync("ReminderItem", item.Id, "Create", JsonSerializer.Serialize(dto), ct);
        
        _logger.LogInformation("Created ReminderItem {Id}: {Title}", item.Id, item.Title);
        
        return MapToDto(item);
    }

    public async Task<ReminderItemDto> UpdateAsync(Guid id, UpdateReminderItemDto dto, CancellationToken ct = default)
    {
        var item = await _repository.GetByIdAsync(id, ct)
            ?? throw new KeyNotFoundException($"ReminderItem {id} not found");

        item.Title = dto.Title;
        item.Description = dto.Description;
        item.Category = dto.Category;
        item.Tags = dto.Tags ?? [];
        item.Assignee = dto.Assignee;
        item.Status = dto.Status;
        item.ScheduleType = dto.ScheduleType;
        item.DueAt = dto.DueAt;
        item.RecurrenceFrequency = dto.RecurrenceFrequency;
        item.CustomIntervalDays = dto.CustomIntervalDays;
        item.Timezone = dto.Timezone ?? "Europe/Madrid";
        item.LeadTimeDays = dto.LeadTimeDays ?? [30];
        item.Metadata = dto.Metadata ?? new();
        item.UpdatedAt = DateTime.UtcNow;

        // Recalcular próxima ocurrencia
        if (item.ScheduleType == ScheduleType.OneTime)
        {
            item.NextOccurrenceAt = item.DueAt;
        }
        else
        {
            item.NextOccurrenceAt = item.CalculateNextOccurrence(DateTime.UtcNow);
        }

        await _repository.UpdateAsync(item, ct);
        await _auditService.LogAsync("ReminderItem", item.Id, "Update", JsonSerializer.Serialize(dto), ct);
        
        return MapToDto(item);
    }

    public async Task DeleteAsync(Guid id, CancellationToken ct = default)
    {
        var item = await _repository.GetByIdAsync(id, ct)
            ?? throw new KeyNotFoundException($"ReminderItem {id} not found");

        item.Status = ItemStatus.Archived;
        item.UpdatedAt = DateTime.UtcNow;
        
        await _repository.UpdateAsync(item, ct);
        await _auditService.LogAsync("ReminderItem", id, "Archive", null, ct);
    }

    public async Task<ReminderItemDto> CompleteAsync(Guid id, CancellationToken ct = default)
    {
        var item = await _repository.GetByIdWithAlertsAsync(id, ct)
            ?? throw new KeyNotFoundException($"ReminderItem {id} not found");

        item.Complete();

        // Resolver alertas abiertas
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

    public async Task<ReminderItemDto> SnoozeAsync(Guid id, DateTime snoozedUntil, CancellationToken ct = default)
    {
        var item = await _repository.GetByIdWithAlertsAsync(id, ct)
            ?? throw new KeyNotFoundException($"ReminderItem {id} not found");

        // Posponer alertas abiertas
        foreach (var alert in item.Alerts.Where(a => a.State is AlertState.Open or AlertState.Acknowledged))
        {
            alert.Snooze(snoozedUntil);
            await _alertRepository.UpdateAsync(alert, ct);
        }

        await _auditService.LogAsync("ReminderItem", item.Id, "Snooze", 
            $"{{\"snoozedUntil\": \"{snoozedUntil}\"}}", ct);

        return MapToDto(item);
    }

    public async Task<ReminderItemDto> PauseAsync(Guid id, CancellationToken ct = default)
    {
        var item = await _repository.GetByIdAsync(id, ct)
            ?? throw new KeyNotFoundException($"ReminderItem {id} not found");

        item.Status = ItemStatus.Paused;
        item.UpdatedAt = DateTime.UtcNow;

        await _repository.UpdateAsync(item, ct);
        await _auditService.LogAsync("ReminderItem", item.Id, "Pause", null, ct);

        return MapToDto(item);
    }

    public async Task<ReminderItemDto> ResumeAsync(Guid id, CancellationToken ct = default)
    {
        var item = await _repository.GetByIdAsync(id, ct)
            ?? throw new KeyNotFoundException($"ReminderItem {id} not found");

        item.Status = ItemStatus.Active;
        item.NextOccurrenceAt = item.CalculateNextOccurrence(DateTime.UtcNow);
        item.UpdatedAt = DateTime.UtcNow;

        await _repository.UpdateAsync(item, ct);
        await _auditService.LogAsync("ReminderItem", item.Id, "Resume", null, ct);

        return MapToDto(item);
    }

    private static ReminderItemDto MapToDto(ReminderItem item)
    {
        return new ReminderItemDto(
            item.Id,
            item.Title,
            item.Description,
            item.Category,
            item.Tags,
            item.Assignee,
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
            item.Alerts?.Count(a => a.State is AlertState.Open or AlertState.Acknowledged) ?? 0
        );
    }
}
