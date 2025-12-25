using JSCHUB.Application.DTOs;
using JSCHUB.Application.Interfaces;
using JSCHUB.Domain.Entities;
using JSCHUB.Domain.Interfaces;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.Extensions.Logging;

namespace JSCHUB.Application.Services;

public class CredencialProyectoService : ICredencialProyectoService
{
    private readonly ICredencialProyectoRepository _repository;
    private readonly IEnlaceProyectoRepository _enlaceRepository;
    private readonly IDataProtector _protector;
    private readonly ILogger<CredencialProyectoService> _logger;

    private const string ProtectorPurpose = "JSCHUB.PasswordManager.Credentials";

    public CredencialProyectoService(
        ICredencialProyectoRepository repository,
        IEnlaceProyectoRepository enlaceRepository,
        IDataProtectionProvider dataProtectionProvider,
        ILogger<CredencialProyectoService> logger)
    {
        _repository = repository;
        _enlaceRepository = enlaceRepository;
        _protector = dataProtectionProvider.CreateProtector(ProtectorPurpose);
        _logger = logger;
    }

    public async Task<CredencialProyectoDto?> GetByIdAsync(Guid id, CancellationToken ct = default)
    {
        var credencial = await _repository.GetByIdWithEnlaceAsync(id, ct);
        return credencial is null ? null : MapToDto(credencial);
    }

    public async Task<CredencialProyectoConPasswordDto?> GetByIdWithPasswordAsync(Guid id, string usuario, CancellationToken ct = default)
    {
        var credencial = await _repository.GetByIdWithEnlaceAsync(id, ct);
        if (credencial is null) return null;

        // Registrar acceso
        credencial.UltimoAcceso = DateTime.UtcNow;
        await _repository.UpdateAsync(credencial, ct);

        _logger.LogInformation("Usuario {Usuario} accedió a credencial {Id} - {Nombre}",
            usuario, credencial.Id, credencial.Nombre);

        return MapToDtoWithPassword(credencial);
    }

    public async Task<IEnumerable<CredencialProyectoDto>> GetByEnlaceIdAsync(Guid enlaceProyectoId, CancellationToken ct = default)
    {
        var credenciales = await _repository.GetByEnlaceIdAsync(enlaceProyectoId, ct);
        return credenciales.Select(MapToDto);
    }

    public async Task<IEnumerable<CredencialProyectoDto>> GetByProyectoIdAsync(Guid proyectoId, CancellationToken ct = default)
    {
        var credenciales = await _repository.GetByProyectoIdAsync(proyectoId, ct);
        return credenciales.Select(MapToDto);
    }

    public async Task<IEnumerable<CredencialProyectoDto>> SearchAsync(
        Guid? proyectoId = null,
        Guid? enlaceProyectoId = null,
        string? searchText = null,
        bool? activa = null,
        CancellationToken ct = default)
    {
        var credenciales = await _repository.SearchAsync(proyectoId, enlaceProyectoId, searchText, activa, ct);
        return credenciales.Select(MapToDto);
    }

    public async Task<CredencialProyectoDto> CreateAsync(CreateCredencialProyectoDto dto, string usuario, CancellationToken ct = default)
    {
        var enlace = await _enlaceRepository.GetByIdAsync(dto.EnlaceProyectoId, ct)
            ?? throw new KeyNotFoundException($"No se encontró el enlace con ID {dto.EnlaceProyectoId}");

        if (string.IsNullOrWhiteSpace(dto.Nombre))
            throw new ArgumentException("El nombre de la credencial es obligatorio");

        if (string.IsNullOrWhiteSpace(dto.Usuario))
            throw new ArgumentException("El nombre de usuario es obligatorio");

        if (string.IsNullOrWhiteSpace(dto.Password))
            throw new ArgumentException("La contraseña es obligatoria");

        // Verificar duplicados
        if (await _repository.ExisteNombreEnEnlaceAsync(dto.EnlaceProyectoId, dto.Nombre, null, ct))
            throw new InvalidOperationException($"Ya existe una credencial con el nombre '{dto.Nombre}' en este enlace");

        // Cifrar la contraseña
        var passwordCifrado = _protector.Protect(dto.Password);

        var credencial = new CredencialProyecto
        {
            Id = Guid.NewGuid(),
            EnlaceProyectoId = dto.EnlaceProyectoId,
            Nombre = dto.Nombre.Trim(),
            Usuario = dto.Usuario.Trim(),
            PasswordCifrado = passwordCifrado,
            Notas = dto.Notas?.Trim(),
            Activa = true,
            CreadoPor = usuario,
            CreadoEl = DateTime.UtcNow,
            ModificadoPor = usuario,
            ModificadoEl = DateTime.UtcNow
        };

        await _repository.AddAsync(credencial, ct);
        _logger.LogInformation("Credencial creada: {Id} - {Nombre} en enlace {EnlaceId} por {Usuario}",
            credencial.Id, credencial.Nombre, credencial.EnlaceProyectoId, usuario);

        // Cargar el enlace para el DTO
        credencial.EnlaceProyecto = enlace;
        return MapToDto(credencial);
    }

    public async Task<CredencialProyectoDto> UpdateAsync(Guid id, UpdateCredencialProyectoDto dto, string usuario, CancellationToken ct = default)
    {
        var credencial = await _repository.GetByIdWithEnlaceAsync(id, ct)
            ?? throw new KeyNotFoundException($"No se encontró la credencial con ID {id}");

        if (string.IsNullOrWhiteSpace(dto.Nombre))
            throw new ArgumentException("El nombre de la credencial es obligatorio");

        if (string.IsNullOrWhiteSpace(dto.Usuario))
            throw new ArgumentException("El nombre de usuario es obligatorio");

        // Verificar duplicados (excluyendo la actual)
        if (await _repository.ExisteNombreEnEnlaceAsync(credencial.EnlaceProyectoId, dto.Nombre, id, ct))
            throw new InvalidOperationException($"Ya existe otra credencial con el nombre '{dto.Nombre}' en este enlace");

        credencial.Nombre = dto.Nombre.Trim();
        credencial.Usuario = dto.Usuario.Trim();
        credencial.Notas = dto.Notas?.Trim();
        credencial.Activa = dto.Activa;
        credencial.ModificadoPor = usuario;
        credencial.ModificadoEl = DateTime.UtcNow;

        // Solo actualizar la contraseña si se proporciona una nueva
        if (!string.IsNullOrEmpty(dto.Password))
        {
            credencial.PasswordCifrado = _protector.Protect(dto.Password);
            _logger.LogInformation("Contraseña actualizada para credencial {Id} por {Usuario}", id, usuario);
        }

        await _repository.UpdateAsync(credencial, ct);
        _logger.LogInformation("Credencial actualizada: {Id} - {Nombre} por {Usuario}",
            credencial.Id, credencial.Nombre, usuario);

        return MapToDto(credencial);
    }

    public async Task DeleteAsync(Guid id, CancellationToken ct = default)
    {
        var credencial = await _repository.GetByIdAsync(id, ct)
            ?? throw new KeyNotFoundException($"No se encontró la credencial con ID {id}");

        await _repository.DeleteAsync(credencial, ct);
        _logger.LogInformation("Credencial eliminada: {Id} - {Nombre}", credencial.Id, credencial.Nombre);
    }

    public async Task<string> GetPasswordAsync(Guid id, string usuario, CancellationToken ct = default)
    {
        var credencial = await _repository.GetByIdAsync(id, ct)
            ?? throw new KeyNotFoundException($"No se encontró la credencial con ID {id}");

        // Registrar acceso
        credencial.UltimoAcceso = DateTime.UtcNow;
        await _repository.UpdateAsync(credencial, ct);

        _logger.LogInformation("Usuario {Usuario} copió contraseña de credencial {Id} - {Nombre}",
            usuario, credencial.Id, credencial.Nombre);

        // Descifrar y devolver
        return _protector.Unprotect(credencial.PasswordCifrado);
    }

    private static CredencialProyectoDto MapToDto(CredencialProyecto c) => new(
        c.Id,
        c.EnlaceProyectoId,
        c.Nombre,
        c.Usuario,
        c.Notas,
        c.Activa,
        c.UltimoAcceso,
        c.CreadoPor,
        c.CreadoEl,
        c.ModificadoPor,
        c.ModificadoEl,
        c.EnlaceProyecto?.Titulo,
        c.EnlaceProyecto?.Url
    );

    private CredencialProyectoConPasswordDto MapToDtoWithPassword(CredencialProyecto c)
    {
        var passwordDescifrado = _protector.Unprotect(c.PasswordCifrado);
        return new(
            c.Id,
            c.EnlaceProyectoId,
            c.Nombre,
            c.Usuario,
            passwordDescifrado,
            c.Notas,
            c.Activa,
            c.UltimoAcceso,
            c.CreadoPor,
            c.CreadoEl,
            c.ModificadoPor,
            c.ModificadoEl,
            c.EnlaceProyecto?.Titulo,
            c.EnlaceProyecto?.Url
        );
    }
}
