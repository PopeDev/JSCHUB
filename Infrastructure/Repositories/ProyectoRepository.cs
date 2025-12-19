using JSCHUB.Domain.Entities;
using JSCHUB.Domain.Enums;
using JSCHUB.Domain.Interfaces;
using JSCHUB.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace JSCHUB.Infrastructure.Repositories;

public class ProyectoRepository : IProyectoRepository
{
    private readonly ReminderDbContext _context;

    public ProyectoRepository(ReminderDbContext context)
    {
        _context = context;
    }

    public async Task<Proyecto?> GetByIdAsync(Guid id, CancellationToken ct = default)
    {
        return await _context.Proyectos.FindAsync(new object[] { id }, ct);
    }

    public async Task<Proyecto?> GetByIdWithEnlacesAsync(Guid id, CancellationToken ct = default)
    {
        return await _context.Proyectos
            .Include(p => p.Enlaces)
            .FirstOrDefaultAsync(p => p.Id == id, ct);
    }

    public async Task<Proyecto?> GetByIdWithRecursosAsync(Guid id, CancellationToken ct = default)
    {
        return await _context.Proyectos
            .Include(p => p.Recursos)
            .FirstOrDefaultAsync(p => p.Id == id, ct);
    }

    public async Task<Proyecto?> GetByIdCompleteAsync(Guid id, CancellationToken ct = default)
    {
        return await _context.Proyectos
            .Include(p => p.Enlaces)
            .Include(p => p.Recursos)
            .FirstOrDefaultAsync(p => p.Id == id, ct);
    }

    public async Task<Proyecto?> GetByNombreAsync(string nombre, CancellationToken ct = default)
    {
        return await _context.Proyectos
            .FirstOrDefaultAsync(p => p.Nombre.ToLower() == nombre.ToLower(), ct);
    }

    public async Task<IEnumerable<Proyecto>> SearchAsync(
        string? searchText = null,
        EstadoProyecto? estado = null,
        string? etiqueta = null,
        bool incluirArchivados = false,
        int skip = 0,
        int take = 50,
        CancellationToken ct = default)
    {
        var query = BuildQuery(searchText, estado, etiqueta, incluirArchivados);

        return await query
            .Include(p => p.Enlaces)
            .Include(p => p.Recursos)
            .OrderByDescending(p => p.ModificadoEl)
            .Skip(skip)
            .Take(take)
            .ToListAsync(ct);
    }

    public async Task<int> CountAsync(
        string? searchText = null,
        EstadoProyecto? estado = null,
        string? etiqueta = null,
        bool incluirArchivados = false,
        CancellationToken ct = default)
    {
        var query = BuildQuery(searchText, estado, etiqueta, incluirArchivados);
        return await query.CountAsync(ct);
    }

    public async Task<Proyecto> AddAsync(Proyecto proyecto, CancellationToken ct = default)
    {
        _context.Proyectos.Add(proyecto);
        await _context.SaveChangesAsync(ct);
        return proyecto;
    }

    public async Task UpdateAsync(Proyecto proyecto, CancellationToken ct = default)
    {
        _context.Entry(proyecto).State = EntityState.Modified;
        await _context.SaveChangesAsync(ct);
    }

    public async Task<bool> ExisteNombreAsync(string nombre, Guid? excluirId = null, CancellationToken ct = default)
    {
        var query = _context.Proyectos.Where(p => p.Nombre.ToLower() == nombre.ToLower().Trim());

        if (excluirId.HasValue)
            query = query.Where(p => p.Id != excluirId.Value);

        return await query.AnyAsync(ct);
    }

    private IQueryable<Proyecto> BuildQuery(
        string? searchText,
        EstadoProyecto? estado,
        string? etiqueta,
        bool incluirArchivados)
    {
        var query = _context.Proyectos.AsQueryable();

        // Por defecto excluir archivados a menos que se indique lo contrario
        if (!incluirArchivados)
            query = query.Where(p => p.Estado != EstadoProyecto.Archivado);

        // Filtrar por estado específico
        if (estado.HasValue)
            query = query.Where(p => p.Estado == estado.Value);

        // Búsqueda por texto
        if (!string.IsNullOrWhiteSpace(searchText))
        {
            var search = searchText.ToLower().Trim();
            query = query.Where(p =>
                p.Nombre.ToLower().Contains(search) ||
                (p.Descripcion != null && p.Descripcion.ToLower().Contains(search)) ||
                (p.Etiquetas != null && p.Etiquetas.ToLower().Contains(search)));
        }

        // Filtrar por etiqueta
        if (!string.IsNullOrWhiteSpace(etiqueta))
        {
            var tag = etiqueta.ToLower().Trim();
            query = query.Where(p => p.Etiquetas != null && p.Etiquetas.ToLower().Contains(tag));
        }

        return query;
    }
}
