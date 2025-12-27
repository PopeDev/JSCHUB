using JSCHUB.Application.Interfaces;
using JSCHUB.Domain.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace JSCHUB.Infrastructure.BackgroundServices;

/// <summary>
/// Servicio en segundo plano que envía notificaciones de eventos próximos.
/// Ejecuta cada 15 minutos y notifica eventos que empiezan entre 15-45 minutos en el futuro.
/// </summary>
public class EventNotificationService : BackgroundService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<EventNotificationService> _logger;
    private readonly TimeSpan _interval = TimeSpan.FromMinutes(15);

    public EventNotificationService(
        IServiceProvider serviceProvider,
        ILogger<EventNotificationService> logger)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("EventNotificationService iniciado. Ejecutando cada {Interval} minutos.", _interval.TotalMinutes);

        // Ejecutar inmediatamente al iniciar
        await ProcessNotificationsAsync(stoppingToken);

        using var timer = new PeriodicTimer(_interval);

        while (await timer.WaitForNextTickAsync(stoppingToken))
        {
            await ProcessNotificationsAsync(stoppingToken);
        }

        _logger.LogInformation("EventNotificationService detenido.");
    }

    private async Task ProcessNotificationsAsync(CancellationToken ct)
    {
        try
        {
            using var scope = _serviceProvider.CreateScope();
            var repository = scope.ServiceProvider.GetRequiredService<IEventRepository>();
            var notificationSender = scope.ServiceProvider.GetRequiredService<INotificationSender>();

            var now = DateTime.UtcNow;
            var fromUtc = now.AddMinutes(15);
            var toUtc = now.AddMinutes(45);

            _logger.LogDebug("Buscando eventos pendientes de notificar entre {From} y {To}",
                fromUtc, toUtc);

            var pendingEvents = await repository.GetPendingNotificationAsync(fromUtc, toUtc, ct);
            var eventsList = pendingEvents.ToList();

            if (eventsList.Count == 0)
            {
                _logger.LogDebug("No hay eventos pendientes de notificar.");
                return;
            }

            _logger.LogInformation("Encontrados {Count} eventos pendientes de notificar.", eventsList.Count);

            var notifiedIds = new List<Guid>();

            foreach (var evento in eventsList)
            {
                try
                {
                    var success = await notificationSender.SendEventReminderAsync(
                        evento.Title,
                        evento.StartUtc,
                        evento.MeetingUrl,
                        ct);

                    if (success)
                    {
                        notifiedIds.Add(evento.Id);
                        _logger.LogInformation("Notificación enviada para evento: {Id} - {Title}",
                            evento.Id, evento.Title);
                    }
                    else
                    {
                        _logger.LogWarning("Fallo al enviar notificación para evento: {Id} - {Title}",
                            evento.Id, evento.Title);
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error al procesar notificación para evento: {Id}", evento.Id);
                }
            }

            // Marcar como notificados los eventos procesados exitosamente
            if (notifiedIds.Count > 0)
            {
                await repository.MarkAsNotifiedAsync(notifiedIds, ct);
                _logger.LogInformation("Marcados {Count} eventos como notificados.", notifiedIds.Count);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error en ProcessNotificationsAsync");
        }
    }
}
