using JSCHUB.Domain.Entities;
using JSCHUB.Domain.Enums;
using JSCHUB.Domain.Interfaces;
using JSCHUB.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace JSCHUB.Infrastructure.Repositories;

public class RecursoProyectoRepository : IRecursoProyectoRepository
{
    private readonly ReminderDbContext _context;

    public RecursoProyectoRepository(ReminderDbContext context)
    {
        _context = context;
    }

    public async Task<RecursoProyecto?> GetByIdAsync(Guid id, CancellationToken ct = default)
    {
        return await _context.RecursosProyecto.FindAsync(new object[] { id }, ct);
    }

    public async Task<IEnumerable<RecursoProyecto>> GetByProyectoIdAsync(Guid proyectoId, CancellationToken ct = default)
    {
        return await _context.RecursosProyecto
            .Where(r => r.ProyectoId == proyectoId)
            .OrderByDescending(r => r.ModificadoEl)
            .ToListAsync(ct);
    }

    public async Task<IEnumerable<RecursoProyecto>> SearchAsync(
        Guid proyectoId,
        string? searchText = null,
        TipoRecurso? tipo = null,
        string? etiqueta = null,
        CancellationToken ct = default)
    {
        var query = _context.RecursosProyecto.Where(r => r.ProyectoId == proyectoId);

        if (!string.IsNullOrWhiteSpace(searchText))
        {
            var search = searchText.ToLower().Trim();
            query = query.Where(r =>
                r.Nombre.ToLower().Contains(search) ||
                (r.Contenido != null && r.Contenido.ToLower().Contains(search)) ||
                (r.Etiquetas != null && r.Etiquetas.ToLower().Contains(search)));
        }

        if (tipo.HasValue)
            query = query.Where(r => r.Tipo == tipo.Value);

        if (!string.IsNullOrWhiteSpace(etiqueta))
        {
            var tag = etiqueta.ToLower().Trim();
            query = query.Where(r => r.Etiquetas != null && r.Etiquetas.ToLower().Contains(tag));
        }

        return await query
            .OrderByDescending(r => r.ModificadoEl)
            .ToListAsync(ct);
    }

    public async Task<RecursoProyecto> AddAsync(RecursoProyecto recurso, CancellationToken ct = default)
    {
        _context.RecursosProyecto.Add(recurso);
        await _context.SaveChangesAsync(ct);
        return recurso;
    }

    public async Task UpdateAsync(RecursoProyecto recurso, CancellationToken ct = default)
    {
        _context.Entry(recurso).State = EntityState.Modified;
        await _context.SaveChangesAsync(ct);
    }

    public async Task DeleteAsync(RecursoProyecto recurso, CancellationToken ct = default)
    {
        _context.RecursosProyecto.Remove(recurso);
        await _context.SaveChangesAsync(ct);
    }
}
