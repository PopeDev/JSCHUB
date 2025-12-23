using JSCHUB.Application.DTOs;
using JSCHUB.Application.Interfaces;
using JSCHUB.Domain.Entities;
using JSCHUB.Domain.Interfaces;
using Microsoft.Extensions.Logging;

namespace JSCHUB.Application.Services;

public class PromptService : IPromptService
{
    private readonly IPromptRepository _repository;
    private readonly ILogger<PromptService> _logger;

    public PromptService(IPromptRepository repository, ILogger<PromptService> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    public async Task<PromptDto?> GetByIdAsync(Guid id, CancellationToken ct = default)
    {
        var prompt = await _repository.GetByIdCompleteAsync(id, ct);
        return prompt == null ? null : MapToDto(prompt);
    }

    public async Task<IEnumerable<PromptDto>> GetAllAsync(CancellationToken ct = default)
    {
        var prompts = await _repository.GetAllAsync(ct);
        return prompts.Select(MapToDto);
    }

    public async Task<IEnumerable<PromptDto>> GetActivosAsync(CancellationToken ct = default)
    {
        var prompts = await _repository.GetActivosAsync(ct);
        return prompts.Select(MapToDto);
    }

    public async Task<IEnumerable<PromptDto>> SearchAsync(
        string? searchText = null,
        Guid? toolId = null,
        Guid? proyectoId = null,
        Guid? tagId = null,
        bool incluirInactivos = false,
        int skip = 0,
        int take = 50,
        CancellationToken ct = default)
    {
        var prompts = await _repository.SearchAsync(
            searchText, toolId, proyectoId, tagId, incluirInactivos, skip, take, ct);
        return prompts.Select(MapToDto);
    }

    public async Task<PromptDto> CreateAsync(CreatePromptDto dto, CancellationToken ct = default)
    {
        var prompt = new Prompt
        {
            Id = Guid.NewGuid(),
            Title = dto.Title,
            Description = dto.Description,
            ProyectoId = dto.ProyectoId,
            CreatedByUserId = dto.CreatedByUserId,
            ToolId = dto.ToolId,
            Activo = true,
            CreadoEl = DateTime.UtcNow,
            ModificadoEl = DateTime.UtcNow
        };

        await _repository.AddAsync(prompt, ct);

        // Agregar tags
        if (dto.TagIds.Any())
        {
            await _repository.UpdateTagsAsync(prompt.Id, dto.TagIds, ct);
        }

        _logger.LogInformation("Prompt creado: {Id} - {Title}", prompt.Id, prompt.Title);

        // Recargar para obtener relaciones
        var created = await _repository.GetByIdCompleteAsync(prompt.Id, ct);
        return MapToDto(created!);
    }

    public async Task<PromptDto> UpdateAsync(Guid id, UpdatePromptDto dto, CancellationToken ct = default)
    {
        var prompt = await _repository.GetByIdAsync(id, ct)
            ?? throw new KeyNotFoundException($"Prompt {id} no encontrado");

        prompt.Title = dto.Title;
        prompt.Description = dto.Description;
        prompt.ProyectoId = dto.ProyectoId;
        prompt.ToolId = dto.ToolId;
        prompt.Activo = dto.Activo;
        prompt.ModificadoEl = DateTime.UtcNow;

        await _repository.UpdateAsync(prompt, ct);

        // Actualizar tags
        await _repository.UpdateTagsAsync(prompt.Id, dto.TagIds, ct);

        // Recargar para obtener relaciones
        var updated = await _repository.GetByIdCompleteAsync(prompt.Id, ct);
        return MapToDto(updated!);
    }

    public async Task ToggleActivoAsync(Guid id, CancellationToken ct = default)
    {
        var prompt = await _repository.GetByIdAsync(id, ct)
            ?? throw new KeyNotFoundException($"Prompt {id} no encontrado");

        prompt.Activo = !prompt.Activo;
        prompt.ModificadoEl = DateTime.UtcNow;
        await _repository.UpdateAsync(prompt, ct);

        _logger.LogInformation("Prompt {Id} - Activo: {Activo}", id, prompt.Activo);
    }

    public async Task DeleteAsync(Guid id, CancellationToken ct = default)
    {
        var prompt = await _repository.GetByIdAsync(id, ct)
            ?? throw new KeyNotFoundException($"Prompt {id} no encontrado");

        await _repository.DeleteAsync(prompt, ct);
        _logger.LogInformation("Prompt eliminado: {Id} - {Title}", id, prompt.Title);
    }

    private static PromptDto MapToDto(Prompt prompt) => new(
        prompt.Id,
        prompt.Title,
        prompt.Description,
        prompt.Activo,
        prompt.ProyectoId,
        prompt.Proyecto?.Nombre,
        prompt.CreatedByUserId,
        prompt.CreatedByUser?.Nombre ?? "Desconocido",
        prompt.ToolId,
        prompt.Tool?.Name ?? "Desconocido",
        prompt.PromptTags.Select(pt => new TagDto(
            pt.Tag.Id,
            pt.Tag.Name,
            pt.Tag.Activo,
            pt.Tag.CreadoEl
        )),
        prompt.CreadoEl,
        prompt.ModificadoEl
    );
}
