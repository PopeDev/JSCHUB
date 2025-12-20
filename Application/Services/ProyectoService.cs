using JSCHUB.Application.DTOs;
using JSCHUB.Application.Interfaces;
using JSCHUB.Domain.Entities;
using JSCHUB.Domain.Enums;
using JSCHUB.Domain.Interfaces;
using Microsoft.Extensions.Logging;

namespace JSCHUB.Application.Services;

public class ProyectoService : IProyectoService
{
    private readonly IProyectoRepository _repository;
    private readonly ILogger<ProyectoService> _logger;

    public ProyectoService(IProyectoRepository repository, ILogger<ProyectoService> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    public async Task<ProyectoDto?> GetByIdAsync(Guid id, CancellationToken ct = default)
    {
        var proyecto = await _repository.GetByIdCompleteAsync(id, ct);
        return proyecto is null ? null : MapToDto(proyecto);
    }

    public async Task<ProyectoDetalleDto?> GetDetalleAsync(Guid id, CancellationToken ct = default)
    {
        var proyecto = await _repository.GetByIdCompleteAsync(id, ct);
        return proyecto is null ? null : MapToDetalleDto(proyecto);
    }

    public async Task<IEnumerable<ProyectoDto>> SearchAsync(
        string? searchText = null,
        EstadoProyecto? estado = null,
        string? etiqueta = null,
        bool incluirArchivados = false,
        int skip = 0,
        int take = 50,
        CancellationToken ct = default)
    {
        var proyectos = await _repository.SearchAsync(searchText, estado, etiqueta, incluirArchivados, skip, take, ct);
        return proyectos.Select(MapToDto);
    }

    public async Task<int> CountAsync(
        string? searchText = null,
        EstadoProyecto? estado = null,
        string? etiqueta = null,
        bool incluirArchivados = false,
        CancellationToken ct = default)
    {
        return await _repository.CountAsync(searchText, estado, etiqueta, incluirArchivados, ct);
    }

    public async Task<ProyectoDto> CreateAsync(CreateProyectoDto dto, string usuario, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(dto.Nombre))
            throw new ArgumentException("El nombre del proyecto es obligatorio");

        var existeNombre = await _repository.ExisteNombreAsync(dto.Nombre, null, ct);
        if (existeNombre)
        {
            _logger.LogWarning("Intento de crear proyecto con nombre duplicado: {Nombre}", dto.Nombre);
            throw new InvalidOperationException($"Ya existe un proyecto con el nombre '{dto.Nombre}'");
        }

        var proyecto = new Proyecto
        {
            Id = Guid.NewGuid(),
            Nombre = dto.Nombre.Trim(),
            Descripcion = dto.Descripcion?.Trim(),
            Estado = dto.Estado ?? EstadoProyecto.Activo,
            EnlacePrincipal = dto.EnlacePrincipal?.Trim(),
            Etiquetas = dto.Etiquetas?.Trim(),
            CreadoPor = usuario,
            CreadoEl = DateTime.UtcNow,
            ModificadoPor = usuario,
            ModificadoEl = DateTime.UtcNow
        };

        await _repository.AddAsync(proyecto, ct);
        _logger.LogInformation("Proyecto creado: {Id} - {Nombre} por {Usuario}", proyecto.Id, proyecto.Nombre, usuario);

        return MapToDto(proyecto);
    }

    public async Task<ProyectoDto> UpdateAsync(Guid id, UpdateProyectoDto dto, string usuario, CancellationToken ct = default)
    {
        var proyecto = await _repository.GetByIdAsync(id, ct)
            ?? throw new KeyNotFoundException($"No se encontró el proyecto con ID {id}");

        if (string.IsNullOrWhiteSpace(dto.Nombre))
            throw new ArgumentException("El nombre del proyecto es obligatorio");

        var existeNombre = await _repository.ExisteNombreAsync(dto.Nombre, id, ct);
        if (existeNombre)
        {
            _logger.LogWarning("Intento de actualizar proyecto con nombre duplicado: {Nombre}", dto.Nombre);
            throw new InvalidOperationException($"Ya existe un proyecto con el nombre '{dto.Nombre}'");
        }

        proyecto.Nombre = dto.Nombre.Trim();
        proyecto.Descripcion = dto.Descripcion?.Trim();
        proyecto.Estado = dto.Estado;
        proyecto.EnlacePrincipal = dto.EnlacePrincipal?.Trim();
        proyecto.Etiquetas = dto.Etiquetas?.Trim();
        proyecto.ModificadoPor = usuario;
        proyecto.ModificadoEl = DateTime.UtcNow;

        await _repository.UpdateAsync(proyecto, ct);
        _logger.LogInformation("Proyecto actualizado: {Id} - {Nombre} por {Usuario}", proyecto.Id, proyecto.Nombre, usuario);

        return MapToDto(proyecto);
    }

    public async Task ArchivarAsync(Guid id, string usuario, CancellationToken ct = default)
    {
        var proyecto = await _repository.GetByIdAsync(id, ct)
            ?? throw new KeyNotFoundException($"No se encontró el proyecto con ID {id}");

        if (proyecto.Estado == EstadoProyecto.Archivado)
            throw new InvalidOperationException("El proyecto ya está archivado");

        proyecto.Archivar(usuario);
        await _repository.UpdateAsync(proyecto, ct);
        _logger.LogInformation("Proyecto archivado: {Id} - {Nombre} por {Usuario}", proyecto.Id, proyecto.Nombre, usuario);
    }

    public async Task ReactivarAsync(Guid id, string usuario, CancellationToken ct = default)
    {
        var proyecto = await _repository.GetByIdAsync(id, ct)
            ?? throw new KeyNotFoundException($"No se encontró el proyecto con ID {id}");

        if (proyecto.Estado != EstadoProyecto.Archivado)
            throw new InvalidOperationException("Solo se pueden reactivar proyectos archivados");

        proyecto.Reactivar(usuario);
        await _repository.UpdateAsync(proyecto, ct);
        _logger.LogInformation("Proyecto reactivado: {Id} - {Nombre} por {Usuario}", proyecto.Id, proyecto.Nombre, usuario);
    }

    public async Task<bool> ExisteNombreAsync(string nombre, Guid? excluirId = null, CancellationToken ct = default)
    {
        return await _repository.ExisteNombreAsync(nombre, excluirId, ct);
    }

    private static ProyectoDto MapToDto(Proyecto p) => new(
        p.Id,
        p.Nombre,
        p.Descripcion,
        p.Estado,
        p.EnlacePrincipal,
        p.Etiquetas,
        p.CreadoPor,
        p.CreadoEl,
        p.ModificadoPor,
        p.ModificadoEl,
        p.Enlaces?.Count ?? 0,
        p.Recursos?.Count ?? 0
    );

    private static ProyectoDetalleDto MapToDetalleDto(Proyecto p) => new(
        p.Id,
        p.Nombre,
        p.Descripcion,
        p.Estado,
        p.EnlacePrincipal,
        p.Etiquetas,
        p.CreadoPor,
        p.CreadoEl,
        p.ModificadoPor,
        p.ModificadoEl,
        p.Enlaces?.OrderBy(e => e.Orden).Select(MapEnlaceToDto) ?? Enumerable.Empty<EnlaceProyectoDto>(),
        p.Recursos?.OrderByDescending(r => r.ModificadoEl).Select(MapRecursoToDto) ?? Enumerable.Empty<RecursoProyectoDto>()
    );

    private static EnlaceProyectoDto MapEnlaceToDto(EnlaceProyecto e) => new(
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

    private static RecursoProyectoDto MapRecursoToDto(RecursoProyecto r) => new(
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
