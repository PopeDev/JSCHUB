using JSCHUB.Application.DTOs;
using JSCHUB.Application.Interfaces;
using JSCHUB.Domain.Entities;
using JSCHUB.Domain.Interfaces;
using Microsoft.Extensions.Logging;

namespace JSCHUB.Application.Services;

public class PersonaService : IPersonaService
{
    private readonly IPersonaRepository _repository;
    private readonly ILogger<PersonaService> _logger;

    public PersonaService(IPersonaRepository repository, ILogger<PersonaService> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    public async Task<PersonaDto?> GetByIdAsync(Guid id, CancellationToken ct = default)
    {
        var persona = await _repository.GetByIdAsync(id, ct);
        return persona == null ? null : MapToDto(persona);
    }

    public async Task<IEnumerable<PersonaDto>> GetAllAsync(CancellationToken ct = default)
    {
        var personas = await _repository.GetAllAsync(ct);
        return personas.Select(MapToDto);
    }

    public async Task<IEnumerable<PersonaDto>> GetActivasAsync(CancellationToken ct = default)
    {
        var personas = await _repository.GetActivasAsync(ct);
        return personas.Select(MapToDto);
    }

    public async Task<PersonaDto> CreateAsync(CreatePersonaDto dto, CancellationToken ct = default)
    {
        var persona = new Persona
        {
            Id = Guid.NewGuid(),
            Nombre = dto.Nombre,
            Email = dto.Email,
            Telefono = dto.Telefono,
            Activo = true
        };

        await _repository.AddAsync(persona, ct);
        _logger.LogInformation("Persona creada: {Id} - {Nombre}", persona.Id, persona.Nombre);
        
        return MapToDto(persona);
    }

    public async Task<PersonaDto> UpdateAsync(Guid id, UpdatePersonaDto dto, CancellationToken ct = default)
    {
        var persona = await _repository.GetByIdAsync(id, ct)
            ?? throw new KeyNotFoundException($"Persona {id} no encontrada");

        persona.Nombre = dto.Nombre;
        persona.Email = dto.Email;
        persona.Telefono = dto.Telefono;
        persona.Activo = dto.Activo;

        await _repository.UpdateAsync(persona, ct);
        
        return MapToDto(persona);
    }

    public async Task ToggleActivoAsync(Guid id, CancellationToken ct = default)
    {
        var persona = await _repository.GetByIdAsync(id, ct)
            ?? throw new KeyNotFoundException($"Persona {id} no encontrada");

        persona.Activo = !persona.Activo;
        await _repository.UpdateAsync(persona, ct);
        
        _logger.LogInformation("Persona {Id} - Activo: {Activo}", id, persona.Activo);
    }

    private static PersonaDto MapToDto(Persona persona) => new(
        persona.Id,
        persona.Nombre,
        persona.Email,
        persona.Telefono,
        persona.Activo
    );
}
