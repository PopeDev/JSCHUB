using JSCHUB.Application.DTOs;
using JSCHUB.Application.Interfaces;
using JSCHUB.Domain.Entities;
using JSCHUB.Domain.Enums;
using JSCHUB.Domain.Interfaces;
using Microsoft.Extensions.Logging;

namespace JSCHUB.Application.Services;

public class AlertService : IAlertService
{
    private readonly IAlertRepository _alertRepository;
    private readonly IReminderItemRepository _reminderRepository;
    private readonly IAuditService _auditService;
    private readonly ILogger<AlertService> _logger;

    public AlertService(
        IAlertRepository alertRepository,
        IReminderItemRepository reminderRepository,
        IAuditService auditService,
        ILogger<AlertService> logger)
    {
        _alertRepository = alertRepository;
        _reminderRepository = reminderRepository;
        _auditService = auditService;
        _logger = logger;
    }

    public async Task<AlertDto?> GetByIdAsync(Guid id, CancellationToken ct = default)
    {
        var alert = await _alertRepository.GetByIdAsync(id, ct);
        return alert == null ? null : MapToDto(alert);
    }

    public async Task<IEnumerable<AlertDto>> SearchAsync(
        AlertState? state = null,
        AlertSeverity? severity = null,
        DateTime? fromDate = null,
        DateTime? toDate = null,
        int skip = 0,
        int take = 50,
        CancellationToken ct = default)
    {
        var alerts = await _alertRepository.SearchAsync(state, severity, fromDate, toDate, skip, take, ct);
        return alerts.Select(MapToDto);
    }

    public async Task<IEnumerable<AlertDto>> GetByReminderItemIdAsync(Guid reminderItemId, CancellationToken ct = default)
    {
        var alerts = await _alertRepository.GetByReminderItemIdAsync(reminderItemId, ct);
        return alerts.Select(MapToDto);
    }

    public async Task<AlertStatsDto> GetStatsAsync(CancellationToken ct = default)
    {
        var totalOpen = await _alertRepository.CountByStateAsync(AlertState.Open, ct);
        var dueToday = await _alertRepository.CountDueTodayAsync(ct);
        var dueNext7Days = await _alertRepository.CountDueInDaysAsync(7, ct);
        var overdue = await _alertRepository.CountOverdueAsync(ct);
        var snoozed = await _alertRepository.CountByStateAsync(AlertState.Snoozed, ct);
        var resolvedToday = await _alertRepository.CountResolvedTodayAsync(ct);

        return new AlertStatsDto(totalOpen, dueToday, dueNext7Days, overdue, snoozed, resolvedToday);
    }

    public async Task AcknowledgeAsync(Guid id, CancellationToken ct = default)
    {
        var alert = await _alertRepository.GetByIdAsync(id, ct)
            ?? throw new KeyNotFoundException($"Alert {id} not found");

        alert.Acknowledge();
        await _alertRepository.UpdateAsync(alert, ct);
        await _auditService.LogAsync("Alert", id, "Acknowledge", null, ct);
    }

    public async Task SnoozeAsync(Guid id, DateTime snoozedUntil, CancellationToken ct = default)
    {
        var alert = await _alertRepository.GetByIdAsync(id, ct)
            ?? throw new KeyNotFoundException($"Alert {id} not found");

        alert.Snooze(snoozedUntil);
        await _alertRepository.UpdateAsync(alert, ct);
        await _auditService.LogAsync("Alert", id, "Snooze", $"{{\"snoozedUntil\": \"{snoozedUntil}\"}}", ct);
    }

    public async Task ResolveAsync(Guid id, CancellationToken ct = default)
    {
        var alert = await _alertRepository.GetByIdAsync(id, ct)
            ?? throw new KeyNotFoundException($"Alert {id} not found");

        alert.Resolve();
        await _alertRepository.UpdateAsync(alert, ct);
        await _auditService.LogAsync("Alert", id, "Resolve", null, ct);
    }

    /// <summary>
    /// Genera alertas para todos los ReminderItems activos que estén dentro de su ventana de aviso
    /// </summary>
    public async Task GenerateAlertsAsync(CancellationToken ct = default)
    {
        var now = DateTime.UtcNow;
        var items = await _reminderRepository.GetActiveAsync(ct);
        var alertsCreated = 0;

        foreach (var item in items)
        {
            if (item.NextOccurrenceAt == null) continue;

            var occurrenceAt = item.NextOccurrenceAt.Value;

            foreach (var leadDays in item.LeadTimeDays)
            {
                var alertTriggerDate = occurrenceAt.AddDays(-leadDays);
                
                // Si ya pasó el trigger date, debemos generar alerta
                if (now >= alertTriggerDate)
                {
                    // Verificar si ya existe alerta para esta ocurrencia
                    var existing = await _alertRepository.FindExistingAsync(item.Id, occurrenceAt, ct);
                    
                    if (existing == null)
                    {
                        var severity = Alert.CalculateSeverity(occurrenceAt, now);
                        var daysUntil = (occurrenceAt - now).Days;
                        
                        var message = daysUntil switch
                        {
                            < 0 => $"⚠️ VENCIDO hace {-daysUntil} días: {item.Title}",
                            0 => $"🔴 HOY vence: {item.Title}",
                            1 => $"🟠 MAÑANA vence: {item.Title}",
                            <= 7 => $"🟡 En {daysUntil} días: {item.Title}",
                            _ => $"📅 En {daysUntil} días: {item.Title}"
                        };

                        var alert = new Alert
                        {
                            Id = Guid.NewGuid(),
                            ReminderItemId = item.Id,
                            State = AlertState.Open,
                            TriggerAt = now,
                            Severity = severity,
                            Message = message,
                            OccurrenceAt = occurrenceAt,
                            CreatedAt = now,
                            UpdatedAt = now
                        };

                        await _alertRepository.AddAsync(alert, ct);
                        alertsCreated++;
                        
                        _logger.LogInformation("Created alert for {Title}, due {Due}", item.Title, occurrenceAt);
                    }
                    else
                    {
                        // Actualizar severidad si cambió
                        var newSeverity = Alert.CalculateSeverity(occurrenceAt, now);
                        if (existing.Severity != newSeverity && existing.State != AlertState.Resolved)
                        {
                            existing.Severity = newSeverity;
                            existing.UpdatedAt = now;
                            await _alertRepository.UpdateAsync(existing, ct);
                        }
                    }
                    
                    break; // Solo una alerta por ocurrencia
                }
            }
        }

        if (alertsCreated > 0)
        {
            _logger.LogInformation("Alert generation complete. Created {Count} new alerts.", alertsCreated);
        }
    }

    private static AlertDto MapToDto(Alert alert)
    {
        return new AlertDto(
            alert.Id,
            alert.ReminderItemId,
            alert.ReminderItem?.Title ?? "Unknown",
            alert.ReminderItem?.Category ?? Category.Other,
            alert.State,
            alert.TriggerAt,
            alert.Severity,
            alert.Message,
            alert.SnoozedUntil,
            alert.OccurrenceAt,
            alert.CreatedAt
        );
    }
}
