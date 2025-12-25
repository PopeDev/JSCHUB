using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace JSCHUB.Infrastructure.Services;

/// <summary>
/// Configuración para Data Protection
/// </summary>
public class DataProtectionSettings
{
    public string KeysPath { get; set; } = "./DataProtectionKeys";
    public string ApplicationName { get; set; } = "JSCHUB-PasswordManager";
    public DataProtectionBackupSettings Backup { get; set; } = new();
}

public class DataProtectionBackupSettings
{
    public bool Enabled { get; set; } = false;
    public string DestinationPath { get; set; } = string.Empty;
    public int RetryCount { get; set; } = 3;
    public int RetryDelayMs { get; set; } = 1000;
}

/// <summary>
/// Servicio de fondo que monitorea cambios en la carpeta de claves de Data Protection
/// y realiza backups automáticos a una ubicación configurada.
/// </summary>
public class DataProtectionKeyWatcherService : BackgroundService
{
    private readonly ILogger<DataProtectionKeyWatcherService> _logger;
    private readonly DataProtectionSettings _settings;
    private FileSystemWatcher? _watcher;
    private readonly SemaphoreSlim _backupLock = new(1, 1);

    public DataProtectionKeyWatcherService(
        ILogger<DataProtectionKeyWatcherService> logger,
        IOptions<DataProtectionSettings> settings)
    {
        _logger = logger;
        _settings = settings.Value;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        if (!_settings.Backup.Enabled)
        {
            _logger.LogInformation("Backup de claves de Data Protection deshabilitado");
            return;
        }

        if (string.IsNullOrWhiteSpace(_settings.Backup.DestinationPath))
        {
            _logger.LogWarning("No se ha configurado la ruta de destino para backup de claves. Backup deshabilitado.");
            return;
        }

        var keysPath = GetAbsolutePath(_settings.KeysPath);

        // Asegurar que la carpeta de claves existe
        if (!Directory.Exists(keysPath))
        {
            _logger.LogInformation("Creando carpeta de claves de Data Protection: {Path}", keysPath);
            Directory.CreateDirectory(keysPath);
        }

        // Asegurar que la carpeta de destino existe
        var destinationPath = GetAbsolutePath(_settings.Backup.DestinationPath);
        if (!Directory.Exists(destinationPath))
        {
            try
            {
                Directory.CreateDirectory(destinationPath);
                _logger.LogInformation("Carpeta de backup creada: {Path}", destinationPath);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "No se pudo crear la carpeta de backup: {Path}", destinationPath);
                return;
            }
        }

        // Realizar backup inicial
        await PerformBackupAsync(keysPath, destinationPath, stoppingToken);

        // Configurar FileSystemWatcher
        _watcher = new FileSystemWatcher(keysPath)
        {
            NotifyFilter = NotifyFilters.FileName |
                          NotifyFilters.LastWrite |
                          NotifyFilters.CreationTime,
            Filter = "*.xml",
            EnableRaisingEvents = true
        };

        _watcher.Created += async (sender, e) => await OnKeyFileChangedAsync(e, destinationPath, stoppingToken);
        _watcher.Changed += async (sender, e) => await OnKeyFileChangedAsync(e, destinationPath, stoppingToken);
        _watcher.Error += OnWatcherError;

        _logger.LogInformation(
            "FileSystemWatcher iniciado. Monitoreando: {KeysPath} → Backup a: {DestinationPath}",
            keysPath, destinationPath);

        // Mantener el servicio activo
        try
        {
            await Task.Delay(Timeout.Infinite, stoppingToken);
        }
        catch (OperationCanceledException)
        {
            _logger.LogInformation("DataProtectionKeyWatcherService detenido");
        }
    }

    private async Task OnKeyFileChangedAsync(FileSystemEventArgs e, string destinationPath, CancellationToken ct)
    {
        try
        {
            // Usar un pequeño delay para asegurar que el archivo está completamente escrito
            await Task.Delay(500, ct);

            await _backupLock.WaitAsync(ct);
            try
            {
                var sourceFile = e.FullPath;
                var destFile = Path.Combine(destinationPath, e.Name!);

                await CopyFileWithRetryAsync(sourceFile, destFile, ct);

                _logger.LogInformation(
                    "Backup de clave realizado: {FileName} → {Destination}",
                    e.Name, destinationPath);
            }
            finally
            {
                _backupLock.Release();
            }
        }
        catch (OperationCanceledException)
        {
            // Ignorar si se cancela
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al realizar backup de clave: {FileName}", e.Name);
        }
    }

    private async Task PerformBackupAsync(string sourcePath, string destinationPath, CancellationToken ct)
    {
        try
        {
            var keyFiles = Directory.GetFiles(sourcePath, "*.xml");
            if (keyFiles.Length == 0)
            {
                _logger.LogInformation("No hay claves para respaldar en {Path}", sourcePath);
                return;
            }

            await _backupLock.WaitAsync(ct);
            try
            {
                foreach (var sourceFile in keyFiles)
                {
                    var fileName = Path.GetFileName(sourceFile);
                    var destFile = Path.Combine(destinationPath, fileName);

                    await CopyFileWithRetryAsync(sourceFile, destFile, ct);
                }

                _logger.LogInformation(
                    "Backup inicial completado: {Count} claves respaldadas en {Destination}",
                    keyFiles.Length, destinationPath);
            }
            finally
            {
                _backupLock.Release();
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al realizar backup inicial de claves");
        }
    }

    private async Task CopyFileWithRetryAsync(string source, string destination, CancellationToken ct)
    {
        var retryCount = _settings.Backup.RetryCount;
        var retryDelay = _settings.Backup.RetryDelayMs;

        for (int attempt = 1; attempt <= retryCount; attempt++)
        {
            try
            {
                // Leer el archivo fuente
                var content = await File.ReadAllBytesAsync(source, ct);

                // Escribir en destino
                await File.WriteAllBytesAsync(destination, content, ct);

                return; // Éxito
            }
            catch (IOException ex) when (attempt < retryCount)
            {
                _logger.LogWarning(
                    "Intento {Attempt}/{RetryCount} fallido al copiar {Source}: {Message}. Reintentando en {Delay}ms...",
                    attempt, retryCount, Path.GetFileName(source), ex.Message, retryDelay);

                await Task.Delay(retryDelay, ct);
                retryDelay *= 2; // Backoff exponencial
            }
        }
    }

    private void OnWatcherError(object sender, ErrorEventArgs e)
    {
        var ex = e.GetException();
        _logger.LogError(ex, "Error en FileSystemWatcher");

        // Intentar reiniciar el watcher
        try
        {
            _watcher?.Dispose();
            _watcher = new FileSystemWatcher(GetAbsolutePath(_settings.KeysPath))
            {
                NotifyFilter = NotifyFilters.FileName |
                              NotifyFilters.LastWrite |
                              NotifyFilters.CreationTime,
                Filter = "*.xml",
                EnableRaisingEvents = true
            };

            var destinationPath = GetAbsolutePath(_settings.Backup.DestinationPath);
            _watcher.Created += async (s, ev) => await OnKeyFileChangedAsync(ev, destinationPath, CancellationToken.None);
            _watcher.Changed += async (s, ev) => await OnKeyFileChangedAsync(ev, destinationPath, CancellationToken.None);
            _watcher.Error += OnWatcherError;

            _logger.LogInformation("FileSystemWatcher reiniciado después de error");
        }
        catch (Exception restartEx)
        {
            _logger.LogError(restartEx, "No se pudo reiniciar FileSystemWatcher");
        }
    }

    private static string GetAbsolutePath(string path)
    {
        if (Path.IsPathRooted(path))
            return path;

        return Path.Combine(Directory.GetCurrentDirectory(), path);
    }

    public override void Dispose()
    {
        _watcher?.Dispose();
        _backupLock.Dispose();
        base.Dispose();
    }
}
