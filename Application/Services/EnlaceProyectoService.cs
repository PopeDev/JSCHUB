using JSCHUB.Application.DTOs;
using JSCHUB.Application.Interfaces;
using JSCHUB.Domain.Entities;
using JSCHUB.Domain.Enums;
using JSCHUB.Domain.Interfaces;
using Microsoft.Extensions.Logging;

namespace JSCHUB.Application.Services;

public class EnlaceProyectoService : IEnlaceProyectoService
{
    private readonly IEnlaceProyectoRepository _repository;
    private readonly IProyectoRepository _proyectoRepository;
    private readonly ILogger<EnlaceProyectoService> _logger;

    public EnlaceProyectoService(
        IEnlaceProyectoRepository repository,
        IProyectoRepository proyectoRepository,
        ILogger<EnlaceProyectoService> logger)
    {
        _repository = repository;
        _proyectoRepository = proyectoRepository;
        _logger = logger;
    }

    public async Task<EnlaceProyectoDto?> GetByIdAsync(Guid id, CancellationToken ct = default)
    {
        var enlace = await _repository.GetByIdAsync(id, ct);
        return enlace is null ? null : MapToDto(enlace);
    }

    public async Task<IEnumerable<EnlaceProyectoDto>> GetByProyectoIdAsync(Guid proyectoId, CancellationToken ct = default)
    {
        var enlaces = await _repository.GetByProyectoIdAsync(proyectoId, ct);
        return enlaces.OrderBy(e => e.Orden).Select(MapToDto);
    }

    public async Task<IEnumerable<EnlaceProyectoDto>> SearchAsync(
        Guid proyectoId,
        string? searchText = null,
        TipoEnlace? tipo = null,
        CancellationToken ct = default)
    {
        var enlaces = await _repository.SearchAsync(proyectoId, searchText, tipo, ct);
        return enlaces.OrderBy(e => e.Orden).Select(MapToDto);
    }

    public async Task<EnlaceProyectoDto> CreateAsync(CreateEnlaceProyectoDto dto, string usuario, CancellationToken ct = default)
    {
        var proyecto = await _proyectoRepository.GetByIdAsync(dto.ProyectoId, ct)
            ?? throw new KeyNotFoundException($"No se encontró el proyecto con ID {dto.ProyectoId}");

        if (proyecto.Estado == EstadoProyecto.Archivado)
            throw new InvalidOperationException("No se pueden añadir enlaces a un proyecto archivado");

        if (string.IsNullOrWhiteSpace(dto.Titulo))
            throw new ArgumentException("El título del enlace es obligatorio");

        if (string.IsNullOrWhiteSpace(dto.Url))
            throw new ArgumentException("La URL del enlace es obligatoria");

        if (!Uri.TryCreate(dto.Url, UriKind.Absolute, out _))
            throw new ArgumentException("La URL no es válida");

        var maxOrden = await _repository.GetMaxOrdenAsync(dto.ProyectoId, ct);

        var enlace = new EnlaceProyecto
        {
            Id = Guid.NewGuid(),
            ProyectoId = dto.ProyectoId,
            Titulo = dto.Titulo.Trim(),
            Url = dto.Url.Trim(),
            Descripcion = dto.Descripcion?.Trim(),
            Tipo = dto.Tipo ?? TipoEnlace.Otro,
            Orden = dto.Orden ?? (maxOrden + 1),
            CreadoPor = usuario,
            CreadoEl = DateTime.UtcNow,
            ModificadoPor = usuario,
            ModificadoEl = DateTime.UtcNow
        };

        await _repository.AddAsync(enlace, ct);
        _logger.LogInformation("Enlace creado: {Id} - {Titulo} en proyecto {ProyectoId} por {Usuario}",
            enlace.Id, enlace.Titulo, enlace.ProyectoId, usuario);

        return MapToDto(enlace);
    }

    public async Task<EnlaceProyectoDto> UpdateAsync(Guid id, UpdateEnlaceProyectoDto dto, string usuario, CancellationToken ct = default)
    {
        var enlace = await _repository.GetByIdAsync(id, ct)
            ?? throw new KeyNotFoundException($"No se encontró el enlace con ID {id}");

        if (string.IsNullOrWhiteSpace(dto.Titulo))
            throw new ArgumentException("El título del enlace es obligatorio");

        if (string.IsNullOrWhiteSpace(dto.Url))
            throw new ArgumentException("La URL del enlace es obligatoria");

        if (!Uri.TryCreate(dto.Url, UriKind.Absolute, out _))
            throw new ArgumentException("La URL no es válida");

        enlace.Titulo = dto.Titulo.Trim();
        enlace.Url = dto.Url.Trim();
        enlace.Descripcion = dto.Descripcion?.Trim();
        enlace.Tipo = dto.Tipo;
        enlace.Orden = dto.Orden;
        enlace.ModificadoPor = usuario;
        enlace.ModificadoEl = DateTime.UtcNow;

        await _repository.UpdateAsync(enlace, ct);
        _logger.LogInformation("Enlace actualizado: {Id} - {Titulo} por {Usuario}", enlace.Id, enlace.Titulo, usuario);

        return MapToDto(enlace);
    }

    public async Task DeleteAsync(Guid id, CancellationToken ct = default)
    {
        var enlace = await _repository.GetByIdAsync(id, ct)
            ?? throw new KeyNotFoundException($"No se encontró el enlace con ID {id}");

        await _repository.DeleteAsync(enlace, ct);
        _logger.LogInformation("Enlace eliminado: {Id} - {Titulo}", enlace.Id, enlace.Titulo);
    }

    private static EnlaceProyectoDto MapToDto(EnlaceProyecto e) => new(
        e.Id,
        e.ProyectoId,
        e.Titulo,
        e.Url,
        e.Descripcion,
        e.Tipo,
        e.Orden,
        e.CreadoPor,
        e.CreadoEl,
        e.ModificadoPor,
        e.ModificadoEl
    );
}
