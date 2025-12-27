namespace JSCHUB.Application.Interfaces;

/// <summary>
/// Interfaz para enviar notificaciones (WhatsApp, Telegram, etc.)
/// </summary>
public interface INotificationSender
{
    /// <summary>
    /// Envía un mensaje de notificación
    /// </summary>
    /// <param name="message">Mensaje a enviar</param>
    /// <param name="ct">Token de cancelación</param>
    /// <returns>True si se envió correctamente, false en caso contrario</returns>
    Task<bool> SendAsync(string message, CancellationToken ct = default);

    /// <summary>
    /// Envía notificación de un evento próximo
    /// </summary>
    /// <param name="title">Título del evento</param>
    /// <param name="startUtc">Fecha/hora de inicio</param>
    /// <param name="meetingUrl">URL de reunión (opcional)</param>
    /// <param name="ct">Token de cancelación</param>
    Task<bool> SendEventReminderAsync(string title, DateTime startUtc, string? meetingUrl, CancellationToken ct = default);
}
