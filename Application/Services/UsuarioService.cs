using JSCHUB.Application.DTOs;
using JSCHUB.Application.Interfaces;
using JSCHUB.Domain.Entities;
using JSCHUB.Domain.Interfaces;
using Microsoft.Extensions.Logging;

namespace JSCHUB.Application.Services;

public class UsuarioService : IUsuarioService
{
    private readonly IUsuarioRepository _repository;
    private readonly ILogger<UsuarioService> _logger;

    public UsuarioService(IUsuarioRepository repository, ILogger<UsuarioService> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    public async Task<UsuarioDto?> GetByIdAsync(Guid id, CancellationToken ct = default)
    {
        var usuario = await _repository.GetByIdAsync(id, ct);
        return usuario == null ? null : MapToDto(usuario);
    }

    public async Task<UsuarioDto?> GetByNombreAsync(string nombre, CancellationToken ct = default)
    {
        var usuario = await _repository.GetByNombreAsync(nombre, ct);
        return usuario == null ? null : MapToDto(usuario);
    }

    public async Task<IEnumerable<UsuarioDto>> GetAllAsync(CancellationToken ct = default)
    {
        var usuarios = await _repository.GetAllAsync(ct);
        return usuarios.Select(MapToDto);
    }

    public async Task<IEnumerable<UsuarioDto>> GetActivosAsync(CancellationToken ct = default)
    {
        var usuarios = await _repository.GetActivosAsync(ct);
        return usuarios.Select(MapToDto);
    }

    public async Task<UsuarioDto> CreateAsync(CreateUsuarioDto dto, CancellationToken ct = default)
    {
        var usuario = new Usuario
        {
            Id = Guid.NewGuid(),
            Nombre = dto.Nombre,
            Email = dto.Email,
            Telefono = dto.Telefono,
            Activo = true
        };

        await _repository.AddAsync(usuario, ct);
        _logger.LogInformation("Usuario creado: {Id} - {Nombre}", usuario.Id, usuario.Nombre);

        return MapToDto(usuario);
    }

    public async Task<UsuarioDto> UpdateAsync(Guid id, UpdateUsuarioDto dto, CancellationToken ct = default)
    {
        var usuario = await _repository.GetByIdAsync(id, ct)
            ?? throw new KeyNotFoundException($"Usuario {id} no encontrado");

        usuario.Nombre = dto.Nombre;
        usuario.Email = dto.Email;
        usuario.Telefono = dto.Telefono;
        usuario.Activo = dto.Activo;

        await _repository.UpdateAsync(usuario, ct);

        return MapToDto(usuario);
    }

    public async Task ToggleActivoAsync(Guid id, CancellationToken ct = default)
    {
        var usuario = await _repository.GetByIdAsync(id, ct)
            ?? throw new KeyNotFoundException($"Usuario {id} no encontrado");

        usuario.Activo = !usuario.Activo;
        await _repository.UpdateAsync(usuario, ct);

        _logger.LogInformation("Usuario {Id} - Activo: {Activo}", id, usuario.Activo);
    }

    public async Task DeleteAsync(Guid id, CancellationToken ct = default)
    {
        var usuario = await _repository.GetByIdAsync(id, ct)
            ?? throw new KeyNotFoundException($"Usuario {id} no encontrado");

        await _repository.DeleteAsync(usuario, ct);
        _logger.LogInformation("Usuario eliminado: {Id} - {Nombre}", id, usuario.Nombre);
    }

    private static UsuarioDto MapToDto(Usuario usuario) => new(
        usuario.Id,
        usuario.Nombre,
        usuario.Email,
        usuario.Telefono,
        usuario.Activo
    );
}
