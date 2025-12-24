using JSCHUB.Application.Interfaces;
using Microsoft.AspNetCore.Components.Authorization;
using System.Security.Claims;

namespace JSCHUB.Infrastructure.Services;

/// <summary>
/// Servicio de autenticación con credenciales hardcodeadas (PoC)
/// Implementa tanto IAuthService como ICurrentUserService para mantener
/// el estado de sesión centralizado.
/// También actúa como AuthenticationStateProvider para Blazor.
/// </summary>
public class AuthService : AuthenticationStateProvider, IAuthService, ICurrentUserService
{
    private static readonly Dictionary<string, string> ValidCredentials = new(StringComparer.OrdinalIgnoreCase)
    {
        ["Javi"] = "123456",
        ["Pope"] = "00000",
        ["Carlos"] = "010203"
    };

    private string? _currentUser;
    private ClaimsPrincipal _currentUserPrincipal = new(new ClaimsIdentity());

    public string? CurrentUserName => _currentUser;

    public bool IsAuthenticated => !string.IsNullOrEmpty(_currentUser);

    public override Task<AuthenticationState> GetAuthenticationStateAsync()
    {
        return Task.FromResult(new AuthenticationState(_currentUserPrincipal));
    }

    public Task<AuthResult> LoginAsync(string username, string password)
    {
        if (string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(password))
        {
            return Task.FromResult(new AuthResult(false, null, "Usuario o contraseña incorrectos"));
        }

        if (ValidCredentials.TryGetValue(username.Trim(), out var expectedPassword) && password == expectedPassword)
        {
            // 1. Actualizar estado interno
            _currentUser = ValidCredentials.Keys.First(k => k.Equals(username.Trim(), StringComparison.OrdinalIgnoreCase));
            
            // 2. Crear ClaimsPrincipal
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.Name, _currentUser)
            };
            var identity = new ClaimsIdentity(claims, "CustomAuth");
            _currentUserPrincipal = new ClaimsPrincipal(identity);

            // 3. Notificar a Blazor
            NotifyAuthenticationStateChanged(GetAuthenticationStateAsync());
            
            return Task.FromResult(new AuthResult(true, _currentUser, null));
        }

        return Task.FromResult(new AuthResult(false, null, "Usuario o contraseña incorrectos"));
    }

    public Task LogoutAsync()
    {
        // 1. Limpiar estado
        _currentUser = null;
        _currentUserPrincipal = new ClaimsPrincipal(new ClaimsIdentity());

        // 2. Notificar a Blazor
        NotifyAuthenticationStateChanged(GetAuthenticationStateAsync());

        return Task.CompletedTask;
    }
}
