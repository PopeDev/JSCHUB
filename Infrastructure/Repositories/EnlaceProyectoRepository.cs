using JSCHUB.Domain.Entities;
using JSCHUB.Domain.Enums;
using JSCHUB.Domain.Interfaces;
using JSCHUB.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace JSCHUB.Infrastructure.Repositories;

public class EnlaceProyectoRepository : IEnlaceProyectoRepository
{
    private readonly ReminderDbContext _context;

    public EnlaceProyectoRepository(ReminderDbContext context)
    {
        _context = context;
    }

    public async Task<EnlaceProyecto?> GetByIdAsync(Guid id, CancellationToken ct = default)
    {
        return await _context.EnlacesProyecto.FindAsync(new object[] { id }, ct);
    }

    public async Task<IEnumerable<EnlaceProyecto>> GetByProyectoIdAsync(Guid proyectoId, CancellationToken ct = default)
    {
        return await _context.EnlacesProyecto
            .Where(e => e.ProyectoId == proyectoId)
            .OrderBy(e => e.Orden)
            .ThenBy(e => e.Titulo)
            .ToListAsync(ct);
    }

    public async Task<IEnumerable<EnlaceProyecto>> SearchAsync(
        Guid proyectoId,
        string? searchText = null,
        TipoEnlace? tipo = null,
        CancellationToken ct = default)
    {
        var query = _context.EnlacesProyecto.Where(e => e.ProyectoId == proyectoId);

        if (!string.IsNullOrWhiteSpace(searchText))
        {
            var search = searchText.ToLower().Trim();
            query = query.Where(e =>
                e.Titulo.ToLower().Contains(search) ||
                e.Url.ToLower().Contains(search) ||
                (e.Descripcion != null && e.Descripcion.ToLower().Contains(search)));
        }

        if (tipo.HasValue)
            query = query.Where(e => e.Tipo == tipo.Value);

        return await query
            .OrderBy(e => e.Orden)
            .ThenBy(e => e.Titulo)
            .ToListAsync(ct);
    }

    public async Task<EnlaceProyecto> AddAsync(EnlaceProyecto enlace, CancellationToken ct = default)
    {
        _context.EnlacesProyecto.Add(enlace);
        await _context.SaveChangesAsync(ct);
        return enlace;
    }

    public async Task UpdateAsync(EnlaceProyecto enlace, CancellationToken ct = default)
    {
        _context.Entry(enlace).State = EntityState.Modified;
        await _context.SaveChangesAsync(ct);
    }

    public async Task DeleteAsync(EnlaceProyecto enlace, CancellationToken ct = default)
    {
        _context.EnlacesProyecto.Remove(enlace);
        await _context.SaveChangesAsync(ct);
    }

    public async Task<int> GetMaxOrdenAsync(Guid proyectoId, CancellationToken ct = default)
    {
        var maxOrden = await _context.EnlacesProyecto
            .Where(e => e.ProyectoId == proyectoId)
            .MaxAsync(e => (int?)e.Orden, ct);

        return maxOrden ?? 0;
    }
}
