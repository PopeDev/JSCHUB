using JSCHUB.Domain.Entities;
using JSCHUB.Domain.Enums;
using JSCHUB.Domain.Interfaces;
using JSCHUB.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace JSCHUB.Infrastructure.Repositories;

public class UsuarioProyectoRepository : IUsuarioProyectoRepository
{
    private readonly ReminderDbContext _context;

    public UsuarioProyectoRepository(ReminderDbContext context)
    {
        _context = context;
    }

    public async Task<UsuarioProyecto?> GetAsync(Guid usuarioId, Guid proyectoId, CancellationToken ct = default)
    {
        return await _context.UsuariosProyectos
            .Include(x => x.Usuario)
            .Include(x => x.Proyecto)
            .FirstOrDefaultAsync(x => x.UsuarioId == usuarioId && x.ProyectoId == proyectoId, ct);
    }

    public async Task<IEnumerable<UsuarioProyecto>> GetProyectosByUsuarioAsync(Guid usuarioId, CancellationToken ct = default)
    {
        return await _context.UsuariosProyectos
            .Include(x => x.Proyecto)
            .Where(x => x.UsuarioId == usuarioId && x.Proyecto.Estado != EstadoProyecto.Archivado)
            .OrderBy(x => x.Proyecto.EsGeneral ? 0 : 1) // Proyecto General primero
            .ThenBy(x => x.Proyecto.Nombre)
            .ToListAsync(ct);
    }

    public async Task<IEnumerable<UsuarioProyecto>> GetUsuariosByProyectoAsync(Guid proyectoId, CancellationToken ct = default)
    {
        return await _context.UsuariosProyectos
            .Include(x => x.Usuario)
            .Where(x => x.ProyectoId == proyectoId && x.Usuario.Activo)
            .OrderBy(x => x.Rol)
            .ThenBy(x => x.Usuario.Nombre)
            .ToListAsync(ct);
    }

    public async Task<bool> TieneAccesoAsync(Guid usuarioId, Guid proyectoId, CancellationToken ct = default)
    {
        return await _context.UsuariosProyectos
            .AnyAsync(x => x.UsuarioId == usuarioId && x.ProyectoId == proyectoId, ct);
    }

    public async Task<RolProyecto?> GetRolAsync(Guid usuarioId, Guid proyectoId, CancellationToken ct = default)
    {
        var asignacion = await _context.UsuariosProyectos
            .FirstOrDefaultAsync(x => x.UsuarioId == usuarioId && x.ProyectoId == proyectoId, ct);
        return asignacion?.Rol;
    }

    public async Task<bool> TieneAccesoATodosAsync(Guid usuarioId, IEnumerable<Guid> proyectoIds, CancellationToken ct = default)
    {
        var ids = proyectoIds.ToList();
        if (!ids.Any()) return true;

        var countAsignados = await _context.UsuariosProyectos
            .CountAsync(x => x.UsuarioId == usuarioId && ids.Contains(x.ProyectoId), ct);

        return countAsignados == ids.Count;
    }

    public async Task<Proyecto?> GetProyectoGeneralAsync(CancellationToken ct = default)
    {
        return await _context.Proyectos
            .FirstOrDefaultAsync(x => x.EsGeneral, ct);
    }

    public async Task<UsuarioProyecto> AddAsync(UsuarioProyecto asignacion, CancellationToken ct = default)
    {
        await _context.UsuariosProyectos.AddAsync(asignacion, ct);
        await _context.SaveChangesAsync(ct);
        return asignacion;
    }

    public async Task UpdateAsync(UsuarioProyecto asignacion, CancellationToken ct = default)
    {
        _context.UsuariosProyectos.Update(asignacion);
        await _context.SaveChangesAsync(ct);
    }

    public async Task DeleteAsync(Guid usuarioId, Guid proyectoId, CancellationToken ct = default)
    {
        var asignacion = await _context.UsuariosProyectos
            .FirstOrDefaultAsync(x => x.UsuarioId == usuarioId && x.ProyectoId == proyectoId, ct);

        if (asignacion != null)
        {
            _context.UsuariosProyectos.Remove(asignacion);
            await _context.SaveChangesAsync(ct);
        }
    }

    public async Task<int> CountGastosNoSaldadosAsync(Guid usuarioId, Guid proyectoId, CancellationToken ct = default)
    {
        return await _context.Gastos
            .Where(g => g.PagadoPorId == usuarioId &&
                        g.Estado != EstadoGasto.Saldado &&
                        g.Estado != EstadoGasto.Anulado &&
                        g.GastosProyecto.Any(gp => gp.ProyectoId == proyectoId))
            .CountAsync(ct);
    }
}
