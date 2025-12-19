namespace JSCHUB.Application.Interfaces;

/// <summary>
/// Servicio para obtener información del usuario actual de la sesión
/// </summary>
public interface ICurrentUserService
{
    /// <summary>
    /// Nombre del usuario actualmente autenticado
    /// </summary>
    string? CurrentUserName { get; }
    
    /// <summary>
    /// Indica si hay un usuario autenticado
    /// </summary>
    bool IsAuthenticated { get; }
}
