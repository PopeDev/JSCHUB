using JSCHUB.Domain.Entities;
using JSCHUB.Domain.Interfaces;
using JSCHUB.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace JSCHUB.Infrastructure.Repositories;

public class CredencialProyectoRepository : ICredencialProyectoRepository
{
    private readonly ReminderDbContext _context;

    public CredencialProyectoRepository(ReminderDbContext context)
    {
        _context = context;
    }

    public async Task<CredencialProyecto?> GetByIdAsync(Guid id, CancellationToken ct = default)
    {
        return await _context.CredencialesProyecto.FindAsync(new object[] { id }, ct);
    }

    public async Task<CredencialProyecto?> GetByIdWithEnlaceAsync(Guid id, CancellationToken ct = default)
    {
        return await _context.CredencialesProyecto
            .Include(c => c.EnlaceProyecto)
            .FirstOrDefaultAsync(c => c.Id == id, ct);
    }

    public async Task<IEnumerable<CredencialProyecto>> GetByEnlaceIdAsync(Guid enlaceProyectoId, CancellationToken ct = default)
    {
        return await _context.CredencialesProyecto
            .Include(c => c.EnlaceProyecto)
            .Where(c => c.EnlaceProyectoId == enlaceProyectoId)
            .OrderBy(c => c.Nombre)
            .ToListAsync(ct);
    }

    public async Task<IEnumerable<CredencialProyecto>> GetByProyectoIdAsync(Guid proyectoId, CancellationToken ct = default)
    {
        return await _context.CredencialesProyecto
            .Include(c => c.EnlaceProyecto)
            .Where(c => c.EnlaceProyecto.ProyectoId == proyectoId)
            .OrderBy(c => c.EnlaceProyecto.Orden)
            .ThenBy(c => c.EnlaceProyecto.Titulo)
            .ThenBy(c => c.Nombre)
            .ToListAsync(ct);
    }

    public async Task<IEnumerable<CredencialProyecto>> SearchAsync(
        Guid? proyectoId = null,
        Guid? enlaceProyectoId = null,
        string? searchText = null,
        bool? activa = null,
        CancellationToken ct = default)
    {
        var query = _context.CredencialesProyecto
            .Include(c => c.EnlaceProyecto)
            .AsQueryable();

        if (proyectoId.HasValue)
            query = query.Where(c => c.EnlaceProyecto.ProyectoId == proyectoId.Value);

        if (enlaceProyectoId.HasValue)
            query = query.Where(c => c.EnlaceProyectoId == enlaceProyectoId.Value);

        if (!string.IsNullOrWhiteSpace(searchText))
        {
            var search = searchText.ToLower().Trim();
            query = query.Where(c =>
                c.Nombre.ToLower().Contains(search) ||
                c.Usuario.ToLower().Contains(search) ||
                (c.Notas != null && c.Notas.ToLower().Contains(search)) ||
                c.EnlaceProyecto.Titulo.ToLower().Contains(search) ||
                c.EnlaceProyecto.Url.ToLower().Contains(search));
        }

        if (activa.HasValue)
            query = query.Where(c => c.Activa == activa.Value);

        return await query
            .OrderBy(c => c.EnlaceProyecto.Orden)
            .ThenBy(c => c.EnlaceProyecto.Titulo)
            .ThenBy(c => c.Nombre)
            .ToListAsync(ct);
    }

    public async Task<CredencialProyecto> AddAsync(CredencialProyecto credencial, CancellationToken ct = default)
    {
        _context.CredencialesProyecto.Add(credencial);
        await _context.SaveChangesAsync(ct);
        return credencial;
    }

    public async Task UpdateAsync(CredencialProyecto credencial, CancellationToken ct = default)
    {
        _context.Entry(credencial).State = EntityState.Modified;
        await _context.SaveChangesAsync(ct);
    }

    public async Task DeleteAsync(CredencialProyecto credencial, CancellationToken ct = default)
    {
        _context.CredencialesProyecto.Remove(credencial);
        await _context.SaveChangesAsync(ct);
    }

    public async Task<bool> ExisteNombreEnEnlaceAsync(Guid enlaceProyectoId, string nombre, Guid? excludeId = null, CancellationToken ct = default)
    {
        var query = _context.CredencialesProyecto
            .Where(c => c.EnlaceProyectoId == enlaceProyectoId && c.Nombre.ToLower() == nombre.ToLower().Trim());

        if (excludeId.HasValue)
            query = query.Where(c => c.Id != excludeId.Value);

        return await query.AnyAsync(ct);
    }
}
