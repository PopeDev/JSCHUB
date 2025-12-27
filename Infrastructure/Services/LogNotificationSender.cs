using JSCHUB.Application.Interfaces;
using Microsoft.Extensions.Logging;

namespace JSCHUB.Infrastructure.Services;

/// <summary>
/// Implementación stub de INotificationSender que escribe en logs.
/// Reemplazar por implementación real de WhatsApp/Telegram cuando esté disponible.
/// </summary>
public class LogNotificationSender : INotificationSender
{
    private readonly ILogger<LogNotificationSender> _logger;

    public LogNotificationSender(ILogger<LogNotificationSender> logger)
    {
        _logger = logger;
    }

    public Task<bool> SendAsync(string message, CancellationToken ct = default)
    {
        _logger.LogInformation("[NOTIFICATION STUB] Mensaje: {Message}", message);
        return Task.FromResult(true);
    }

    public Task<bool> SendEventReminderAsync(string title, DateTime startUtc, string? meetingUrl, CancellationToken ct = default)
    {
        var localTime = startUtc.ToLocalTime();
        var message = $"Recordatorio: '{title}' comienza a las {localTime:HH:mm} ({localTime:dd/MM/yyyy})";

        if (!string.IsNullOrEmpty(meetingUrl))
        {
            message += $"\nUnirse: {meetingUrl}";
        }

        _logger.LogInformation("[NOTIFICATION STUB] Evento recordatorio enviado:\n{Message}", message);

        return Task.FromResult(true);
    }
}
