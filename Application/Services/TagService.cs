using JSCHUB.Application.DTOs;
using JSCHUB.Application.Interfaces;
using JSCHUB.Domain.Entities;
using JSCHUB.Domain.Interfaces;
using Microsoft.Extensions.Logging;

namespace JSCHUB.Application.Services;

public class TagService : ITagService
{
    private readonly ITagRepository _repository;
    private readonly ILogger<TagService> _logger;

    public TagService(ITagRepository repository, ILogger<TagService> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    public async Task<TagDto?> GetByIdAsync(Guid id, CancellationToken ct = default)
    {
        var tag = await _repository.GetByIdAsync(id, ct);
        return tag == null ? null : MapToDto(tag);
    }

    public async Task<IEnumerable<TagDto>> GetAllAsync(CancellationToken ct = default)
    {
        var tags = await _repository.GetAllAsync(ct);
        return tags.Select(MapToDto);
    }

    public async Task<IEnumerable<TagDto>> GetActivosAsync(CancellationToken ct = default)
    {
        var tags = await _repository.GetActivosAsync(ct);
        return tags.Select(MapToDto);
    }

    public async Task<TagDto> CreateAsync(CreateTagDto dto, CancellationToken ct = default)
    {
        var tag = new Tag
        {
            Id = Guid.NewGuid(),
            Name = dto.Name,
            Activo = true,
            CreadoEl = DateTime.UtcNow
        };

        await _repository.AddAsync(tag, ct);
        _logger.LogInformation("Tag creado: {Id} - {Name}", tag.Id, tag.Name);

        return MapToDto(tag);
    }

    public async Task<TagDto> UpdateAsync(Guid id, UpdateTagDto dto, CancellationToken ct = default)
    {
        var tag = await _repository.GetByIdAsync(id, ct)
            ?? throw new KeyNotFoundException($"Tag {id} no encontrado");

        tag.Name = dto.Name;
        tag.Activo = dto.Activo;

        await _repository.UpdateAsync(tag, ct);

        return MapToDto(tag);
    }

    public async Task ToggleActivoAsync(Guid id, CancellationToken ct = default)
    {
        var tag = await _repository.GetByIdAsync(id, ct)
            ?? throw new KeyNotFoundException($"Tag {id} no encontrado");

        tag.Activo = !tag.Activo;
        await _repository.UpdateAsync(tag, ct);

        _logger.LogInformation("Tag {Id} - Activo: {Activo}", id, tag.Activo);
    }

    public async Task DeleteAsync(Guid id, CancellationToken ct = default)
    {
        var tag = await _repository.GetByIdAsync(id, ct)
            ?? throw new KeyNotFoundException($"Tag {id} no encontrado");

        await _repository.DeleteAsync(tag, ct);
        _logger.LogInformation("Tag eliminado: {Id} - {Name}", id, tag.Name);
    }

    private static TagDto MapToDto(Tag tag) => new(
        tag.Id,
        tag.Name,
        tag.Activo,
        tag.CreadoEl
    );
}
