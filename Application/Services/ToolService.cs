using JSCHUB.Application.DTOs;
using JSCHUB.Application.Interfaces;
using JSCHUB.Domain.Entities;
using JSCHUB.Domain.Interfaces;
using Microsoft.Extensions.Logging;

namespace JSCHUB.Application.Services;

public class ToolService : IToolService
{
    private readonly IToolRepository _repository;
    private readonly ILogger<ToolService> _logger;

    public ToolService(IToolRepository repository, ILogger<ToolService> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    public async Task<ToolDto?> GetByIdAsync(Guid id, CancellationToken ct = default)
    {
        var tool = await _repository.GetByIdAsync(id, ct);
        return tool == null ? null : MapToDto(tool);
    }

    public async Task<IEnumerable<ToolDto>> GetAllAsync(CancellationToken ct = default)
    {
        var tools = await _repository.GetAllAsync(ct);
        return tools.Select(MapToDto);
    }

    public async Task<IEnumerable<ToolDto>> GetActivosAsync(CancellationToken ct = default)
    {
        var tools = await _repository.GetActivosAsync(ct);
        return tools.Select(MapToDto);
    }

    public async Task<ToolDto> CreateAsync(CreateToolDto dto, CancellationToken ct = default)
    {
        var tool = new Tool
        {
            Id = Guid.NewGuid(),
            Name = dto.Name,
            Activo = true,
            CreadoEl = DateTime.UtcNow
        };

        await _repository.AddAsync(tool, ct);
        _logger.LogInformation("Tool creado: {Id} - {Name}", tool.Id, tool.Name);

        return MapToDto(tool);
    }

    public async Task<ToolDto> UpdateAsync(Guid id, UpdateToolDto dto, CancellationToken ct = default)
    {
        var tool = await _repository.GetByIdAsync(id, ct)
            ?? throw new KeyNotFoundException($"Tool {id} no encontrado");

        tool.Name = dto.Name;
        tool.Activo = dto.Activo;

        await _repository.UpdateAsync(tool, ct);

        return MapToDto(tool);
    }

    public async Task ToggleActivoAsync(Guid id, CancellationToken ct = default)
    {
        var tool = await _repository.GetByIdAsync(id, ct)
            ?? throw new KeyNotFoundException($"Tool {id} no encontrado");

        tool.Activo = !tool.Activo;
        await _repository.UpdateAsync(tool, ct);

        _logger.LogInformation("Tool {Id} - Activo: {Activo}", id, tool.Activo);
    }

    public async Task DeleteAsync(Guid id, CancellationToken ct = default)
    {
        var tool = await _repository.GetByIdAsync(id, ct)
            ?? throw new KeyNotFoundException($"Tool {id} no encontrado");

        await _repository.DeleteAsync(tool, ct);
        _logger.LogInformation("Tool eliminado: {Id} - {Name}", id, tool.Name);
    }

    private static ToolDto MapToDto(Tool tool) => new(
        tool.Id,
        tool.Name,
        tool.Activo,
        tool.CreadoEl
    );
}
