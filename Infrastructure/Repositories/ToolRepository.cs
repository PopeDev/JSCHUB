using JSCHUB.Domain.Entities;
using JSCHUB.Domain.Interfaces;
using Microsoft.EntityFrameworkCore;
using JSCHUB.Infrastructure.Data;

namespace JSCHUB.Infrastructure.Repositories;

public class ToolRepository : IToolRepository
{
    private readonly ReminderDbContext _context;

    public ToolRepository(ReminderDbContext context)
    {
        _context = context;
    }

    public async Task<Tool?> GetByIdAsync(Guid id, CancellationToken ct = default)
    {
        return await _context.Tools.FindAsync([id], ct);
    }

    public async Task<Tool?> GetByNameAsync(string name, CancellationToken ct = default)
    {
        return await _context.Tools
            .FirstOrDefaultAsync(x => x.Name.ToLower() == name.ToLower(), ct);
    }

    public async Task<IEnumerable<Tool>> GetAllAsync(CancellationToken ct = default)
    {
        return await _context.Tools
            .OrderBy(x => x.Name)
            .ToListAsync(ct);
    }

    public async Task<IEnumerable<Tool>> GetActivosAsync(CancellationToken ct = default)
    {
        return await _context.Tools
            .Where(x => x.Activo)
            .OrderBy(x => x.Name)
            .ToListAsync(ct);
    }

    public async Task<Tool> AddAsync(Tool tool, CancellationToken ct = default)
    {
        await _context.Tools.AddAsync(tool, ct);
        await _context.SaveChangesAsync(ct);
        return tool;
    }

    public async Task UpdateAsync(Tool tool, CancellationToken ct = default)
    {
        _context.Tools.Update(tool);
        await _context.SaveChangesAsync(ct);
    }

    public async Task DeleteAsync(Tool tool, CancellationToken ct = default)
    {
        _context.Tools.Remove(tool);
        await _context.SaveChangesAsync(ct);
    }

    public async Task<bool> ExistsAsync(Guid id, CancellationToken ct = default)
    {
        return await _context.Tools.AnyAsync(x => x.Id == id, ct);
    }
}
