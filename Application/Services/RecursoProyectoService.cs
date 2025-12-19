using JSCHUB.Application.DTOs;
using JSCHUB.Application.Interfaces;
using JSCHUB.Domain.Entities;
using JSCHUB.Domain.Enums;
using JSCHUB.Domain.Interfaces;
using Microsoft.Extensions.Logging;

namespace JSCHUB.Application.Services;

public class RecursoProyectoService : IRecursoProyectoService
{
    private readonly IRecursoProyectoRepository _repository;
    private readonly IProyectoRepository _proyectoRepository;
    private readonly ILogger<RecursoProyectoService> _logger;

    public RecursoProyectoService(
        IRecursoProyectoRepository repository,
        IProyectoRepository proyectoRepository,
        ILogger<RecursoProyectoService> logger)
    {
        _repository = repository;
        _proyectoRepository = proyectoRepository;
        _logger = logger;
    }

    public async Task<RecursoProyectoDto?> GetByIdAsync(Guid id, CancellationToken ct = default)
    {
        var recurso = await _repository.GetByIdAsync(id, ct);
        return recurso is null ? null : MapToDto(recurso);
    }

    public async Task<IEnumerable<RecursoProyectoDto>> GetByProyectoIdAsync(Guid proyectoId, CancellationToken ct = default)
    {
        var recursos = await _repository.GetByProyectoIdAsync(proyectoId, ct);
        return recursos.OrderByDescending(r => r.ModificadoEl).Select(MapToDto);
    }

    public async Task<IEnumerable<RecursoProyectoDto>> SearchAsync(
        Guid proyectoId,
        string? searchText = null,
        TipoRecurso? tipo = null,
        string? etiqueta = null,
        CancellationToken ct = default)
    {
        var recursos = await _repository.SearchAsync(proyectoId, searchText, tipo, etiqueta, ct);
        return recursos.OrderByDescending(r => r.ModificadoEl).Select(MapToDto);
    }

    public async Task<RecursoProyectoDto> CreateAsync(CreateRecursoProyectoDto dto, string usuario, CancellationToken ct = default)
    {
        var proyecto = await _proyectoRepository.GetByIdAsync(dto.ProyectoId, ct)
            ?? throw new KeyNotFoundException($"No se encontró el proyecto con ID {dto.ProyectoId}");

        if (proyecto.Estado == EstadoProyecto.Archivado)
            throw new InvalidOperationException("No se pueden añadir recursos a un proyecto archivado");

        if (string.IsNullOrWhiteSpace(dto.Nombre))
            throw new ArgumentException("El nombre del recurso es obligatorio");

        // Validación según tipo
        ValidarSegunTipo(dto.Tipo, dto.Url, dto.Contenido);

        var recurso = new RecursoProyecto
        {
            Id = Guid.NewGuid(),
            ProyectoId = dto.ProyectoId,
            Nombre = dto.Nombre.Trim(),
            Tipo = dto.Tipo,
            Url = dto.Url?.Trim(),
            Contenido = dto.Contenido?.Trim(),
            Etiquetas = dto.Etiquetas?.Trim(),
            CreadoPor = usuario,
            CreadoEl = DateTime.UtcNow,
            ModificadoPor = usuario,
            ModificadoEl = DateTime.UtcNow
        };

        await _repository.AddAsync(recurso, ct);
        _logger.LogInformation("Recurso creado: {Id} - {Nombre} en proyecto {ProyectoId} por {Usuario}",
            recurso.Id, recurso.Nombre, recurso.ProyectoId, usuario);

        return MapToDto(recurso);
    }

    public async Task<RecursoProyectoDto> UpdateAsync(Guid id, UpdateRecursoProyectoDto dto, string usuario, CancellationToken ct = default)
    {
        var recurso = await _repository.GetByIdAsync(id, ct)
            ?? throw new KeyNotFoundException($"No se encontró el recurso con ID {id}");

        if (string.IsNullOrWhiteSpace(dto.Nombre))
            throw new ArgumentException("El nombre del recurso es obligatorio");

        // Validación según tipo
        ValidarSegunTipo(dto.Tipo, dto.Url, dto.Contenido);

        recurso.Nombre = dto.Nombre.Trim();
        recurso.Tipo = dto.Tipo;
        recurso.Url = dto.Url?.Trim();
        recurso.Contenido = dto.Contenido?.Trim();
        recurso.Etiquetas = dto.Etiquetas?.Trim();
        recurso.ModificadoPor = usuario;
        recurso.ModificadoEl = DateTime.UtcNow;

        await _repository.UpdateAsync(recurso, ct);
        _logger.LogInformation("Recurso actualizado: {Id} - {Nombre} por {Usuario}", recurso.Id, recurso.Nombre, usuario);

        return MapToDto(recurso);
    }

    public async Task DeleteAsync(Guid id, CancellationToken ct = default)
    {
        var recurso = await _repository.GetByIdAsync(id, ct)
            ?? throw new KeyNotFoundException($"No se encontró el recurso con ID {id}");

        await _repository.DeleteAsync(recurso, ct);
        _logger.LogInformation("Recurso eliminado: {Id} - {Nombre}", recurso.Id, recurso.Nombre);
    }

    private static void ValidarSegunTipo(TipoRecurso tipo, string? url, string? contenido)
    {
        if (tipo == TipoRecurso.Enlace || tipo == TipoRecurso.DocumentoExterno)
        {
            if (string.IsNullOrWhiteSpace(url))
                throw new ArgumentException("La URL es obligatoria para recursos de tipo Enlace o Documento externo");

            if (!Uri.TryCreate(url, UriKind.Absolute, out _))
                throw new ArgumentException("La URL no es válida");
        }

        if (tipo == TipoRecurso.Nota)
        {
            if (string.IsNullOrWhiteSpace(contenido))
                throw new ArgumentException("El contenido es obligatorio para recursos de tipo Nota");
        }
    }

    private static RecursoProyectoDto MapToDto(RecursoProyecto r) => new(
        r.Id,
        r.ProyectoId,
        r.Nombre,
        r.Tipo,
        r.Url,
        r.Contenido,
        r.Etiquetas,
        r.CreadoPor,
        r.CreadoEl,
        r.ModificadoPor,
        r.ModificadoEl
    );
}
