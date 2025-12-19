using JSCHUB.Application.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace JSCHUB.Infrastructure.BackgroundServices;

/// <summary>
/// Background service that periodically generates alerts for reminders within their lead time window
/// </summary>
public class AlertGeneratorService : BackgroundService
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<AlertGeneratorService> _logger;
    private readonly TimeSpan _interval = TimeSpan.FromMinutes(15);

    public AlertGeneratorService(
        IServiceScopeFactory scopeFactory,
        ILogger<AlertGeneratorService> logger)
    {
        _scopeFactory = scopeFactory;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("AlertGeneratorService started. Running every {Interval} minutes.", _interval.TotalMinutes);

        // Run immediately on startup
        await GenerateAlertsAsync(stoppingToken);

        using var timer = new PeriodicTimer(_interval);
        
        while (await timer.WaitForNextTickAsync(stoppingToken))
        {
            await GenerateAlertsAsync(stoppingToken);
        }
    }

    private async Task GenerateAlertsAsync(CancellationToken ct)
    {
        try
        {
            using var scope = _scopeFactory.CreateScope();
            var alertService = scope.ServiceProvider.GetRequiredService<IAlertService>();
            
            _logger.LogDebug("Running alert generation...");
            await alertService.GenerateAlertsAsync(ct);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating alerts");
        }
    }
}
