using JSCHUB.Domain.Entities;
using JSCHUB.Domain.Interfaces;
using Microsoft.EntityFrameworkCore;
using JSCHUB.Infrastructure.Data;

namespace JSCHUB.Infrastructure.Repositories;

public class PersonaRepository : IPersonaRepository
{
    private readonly ReminderDbContext _context;

    public PersonaRepository(ReminderDbContext context)
    {
        _context = context;
    }

    public async Task<Persona?> GetByIdAsync(Guid id, CancellationToken ct = default)
    {
        return await _context.Personas.FindAsync([id], ct);
    }

    public async Task<IEnumerable<Persona>> GetAllAsync(CancellationToken ct = default)
    {
        return await _context.Personas
            .OrderBy(x => x.Nombre)
            .ToListAsync(ct);
    }

    public async Task<IEnumerable<Persona>> GetActivasAsync(CancellationToken ct = default)
    {
        return await _context.Personas
            .Where(x => x.Activo)
            .OrderBy(x => x.Nombre)
            .ToListAsync(ct);
    }

    public async Task<Persona> AddAsync(Persona persona, CancellationToken ct = default)
    {
        await _context.Personas.AddAsync(persona, ct);
        await _context.SaveChangesAsync(ct);
        return persona;
    }

    public async Task UpdateAsync(Persona persona, CancellationToken ct = default)
    {
        _context.Personas.Update(persona);
        await _context.SaveChangesAsync(ct);
    }

    public async Task<bool> ExistsAsync(Guid id, CancellationToken ct = default)
    {
        return await _context.Personas.AnyAsync(x => x.Id == id, ct);
    }
}
