using JSCHUB.Domain.Entities;
using JSCHUB.Domain.Interfaces;
using Microsoft.EntityFrameworkCore;
using JSCHUB.Infrastructure.Data;

namespace JSCHUB.Infrastructure.Repositories;

public class UsuarioRepository : IUsuarioRepository
{
    private readonly ReminderDbContext _context;

    public UsuarioRepository(ReminderDbContext context)
    {
        _context = context;
    }

    public async Task<Usuario?> GetByIdAsync(Guid id, CancellationToken ct = default)
    {
        return await _context.Usuarios.FindAsync([id], ct);
    }

    public async Task<Usuario?> GetByNombreAsync(string nombre, CancellationToken ct = default)
    {
        return await _context.Usuarios
            .FirstOrDefaultAsync(x => x.Nombre.ToLower() == nombre.ToLower(), ct);
    }

    public async Task<IEnumerable<Usuario>> GetAllAsync(CancellationToken ct = default)
    {
        return await _context.Usuarios
            .OrderBy(x => x.Nombre)
            .ToListAsync(ct);
    }

    public async Task<IEnumerable<Usuario>> GetActivosAsync(CancellationToken ct = default)
    {
        return await _context.Usuarios
            .Where(x => x.Activo)
            .OrderBy(x => x.Nombre)
            .ToListAsync(ct);
    }

    public async Task<Usuario> AddAsync(Usuario usuario, CancellationToken ct = default)
    {
        await _context.Usuarios.AddAsync(usuario, ct);
        await _context.SaveChangesAsync(ct);
        return usuario;
    }

    public async Task UpdateAsync(Usuario usuario, CancellationToken ct = default)
    {
        _context.Usuarios.Update(usuario);
        await _context.SaveChangesAsync(ct);
    }

    public async Task DeleteAsync(Usuario usuario, CancellationToken ct = default)
    {
        _context.Usuarios.Remove(usuario);
        await _context.SaveChangesAsync(ct);
    }

    public async Task<bool> ExistsAsync(Guid id, CancellationToken ct = default)
    {
        return await _context.Usuarios.AnyAsync(x => x.Id == id, ct);
    }
}
