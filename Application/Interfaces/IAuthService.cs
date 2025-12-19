namespace JSCHUB.Application.Interfaces;

/// <summary>
/// Servicio de autenticación
/// </summary>
public interface IAuthService
{
    /// <summary>
    /// Intenta iniciar sesión con las credenciales proporcionadas
    /// </summary>
    Task<AuthResult> LoginAsync(string username, string password);
    
    /// <summary>
    /// Cierra la sesión del usuario actual
    /// </summary>
    Task LogoutAsync();
}

/// <summary>
/// Resultado de la operación de login
/// </summary>
public record AuthResult(bool Success, string? UserName, string? ErrorMessage);
