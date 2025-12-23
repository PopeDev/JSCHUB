using JSCHUB.Domain.Entities;
using JSCHUB.Domain.Interfaces;
using Microsoft.EntityFrameworkCore;
using JSCHUB.Infrastructure.Data;

namespace JSCHUB.Infrastructure.Repositories;

public class TagRepository : ITagRepository
{
    private readonly ReminderDbContext _context;

    public TagRepository(ReminderDbContext context)
    {
        _context = context;
    }

    public async Task<Tag?> GetByIdAsync(Guid id, CancellationToken ct = default)
    {
        return await _context.Tags.FindAsync([id], ct);
    }

    public async Task<Tag?> GetByNameAsync(string name, CancellationToken ct = default)
    {
        return await _context.Tags
            .FirstOrDefaultAsync(x => x.Name.ToLower() == name.ToLower(), ct);
    }

    public async Task<IEnumerable<Tag>> GetAllAsync(CancellationToken ct = default)
    {
        return await _context.Tags
            .OrderBy(x => x.Name)
            .ToListAsync(ct);
    }

    public async Task<IEnumerable<Tag>> GetActivosAsync(CancellationToken ct = default)
    {
        return await _context.Tags
            .Where(x => x.Activo)
            .OrderBy(x => x.Name)
            .ToListAsync(ct);
    }

    public async Task<IEnumerable<Tag>> GetByIdsAsync(IEnumerable<Guid> ids, CancellationToken ct = default)
    {
        return await _context.Tags
            .Where(x => ids.Contains(x.Id))
            .OrderBy(x => x.Name)
            .ToListAsync(ct);
    }

    public async Task<Tag> AddAsync(Tag tag, CancellationToken ct = default)
    {
        await _context.Tags.AddAsync(tag, ct);
        await _context.SaveChangesAsync(ct);
        return tag;
    }

    public async Task UpdateAsync(Tag tag, CancellationToken ct = default)
    {
        _context.Tags.Update(tag);
        await _context.SaveChangesAsync(ct);
    }

    public async Task DeleteAsync(Tag tag, CancellationToken ct = default)
    {
        _context.Tags.Remove(tag);
        await _context.SaveChangesAsync(ct);
    }

    public async Task<bool> ExistsAsync(Guid id, CancellationToken ct = default)
    {
        return await _context.Tags.AnyAsync(x => x.Id == id, ct);
    }
}
