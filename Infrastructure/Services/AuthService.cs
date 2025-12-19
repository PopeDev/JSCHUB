using JSCHUB.Application.Interfaces;

namespace JSCHUB.Infrastructure.Services;

/// <summary>
/// Servicio de autenticación con credenciales hardcodeadas (PoC)
/// Implementa tanto IAuthService como ICurrentUserService para mantener
/// el estado de sesión centralizado.
/// </summary>
public class AuthService : IAuthService, ICurrentUserService
{
    /// <summary>
    /// Usuarios válidos con sus contraseñas
    /// </summary>
    private static readonly Dictionary<string, string> ValidCredentials = new(StringComparer.OrdinalIgnoreCase)
    {
        ["Javi"] = "123456",
        ["Pope"] = "00000",
        ["Carlos"] = "010203"
    };

    /// <summary>
    /// Usuario actual de la sesión (almacenamiento en memoria)
    /// </summary>
    private string? _currentUser;

    /// <inheritdoc />
    public string? CurrentUserName => _currentUser;

    /// <inheritdoc />
    public bool IsAuthenticated => !string.IsNullOrEmpty(_currentUser);

    /// <inheritdoc />
    public Task<AuthResult> LoginAsync(string username, string password)
    {
        if (string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(password))
        {
            return Task.FromResult(new AuthResult(false, null, "Usuario o contraseña incorrectos"));
        }

        // Buscar usuario válido (case-insensitive para el nombre)
        if (ValidCredentials.TryGetValue(username.Trim(), out var expectedPassword) 
            && password == expectedPassword)
        {
            // Guardar el nombre con la capitalización correcta
            _currentUser = ValidCredentials.Keys.First(k => 
                k.Equals(username.Trim(), StringComparison.OrdinalIgnoreCase));
            
            return Task.FromResult(new AuthResult(true, _currentUser, null));
        }

        return Task.FromResult(new AuthResult(false, null, "Usuario o contraseña incorrectos"));
    }

    /// <inheritdoc />
    public Task LogoutAsync()
    {
        _currentUser = null;
        return Task.CompletedTask;
    }
}
