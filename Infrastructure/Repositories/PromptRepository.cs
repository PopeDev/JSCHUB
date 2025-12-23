using JSCHUB.Domain.Entities;
using JSCHUB.Domain.Interfaces;
using Microsoft.EntityFrameworkCore;
using JSCHUB.Infrastructure.Data;

namespace JSCHUB.Infrastructure.Repositories;

public class PromptRepository : IPromptRepository
{
    private readonly ReminderDbContext _context;

    public PromptRepository(ReminderDbContext context)
    {
        _context = context;
    }

    public async Task<Prompt?> GetByIdAsync(Guid id, CancellationToken ct = default)
    {
        return await _context.Prompts.FindAsync([id], ct);
    }

    public async Task<Prompt?> GetByIdCompleteAsync(Guid id, CancellationToken ct = default)
    {
        return await _context.Prompts
            .Include(p => p.Tool)
            .Include(p => p.CreatedByUser)
            .Include(p => p.Proyecto)
            .Include(p => p.PromptTags)
                .ThenInclude(pt => pt.Tag)
            .FirstOrDefaultAsync(p => p.Id == id, ct);
    }

    public async Task<IEnumerable<Prompt>> GetAllAsync(CancellationToken ct = default)
    {
        return await _context.Prompts
            .Include(p => p.Tool)
            .Include(p => p.CreatedByUser)
            .Include(p => p.Proyecto)
            .Include(p => p.PromptTags)
                .ThenInclude(pt => pt.Tag)
            .OrderByDescending(p => p.ModificadoEl)
            .ToListAsync(ct);
    }

    public async Task<IEnumerable<Prompt>> GetActivosAsync(CancellationToken ct = default)
    {
        return await _context.Prompts
            .Include(p => p.Tool)
            .Include(p => p.CreatedByUser)
            .Include(p => p.Proyecto)
            .Include(p => p.PromptTags)
                .ThenInclude(pt => pt.Tag)
            .Where(p => p.Activo)
            .OrderByDescending(p => p.ModificadoEl)
            .ToListAsync(ct);
    }

    public async Task<IEnumerable<Prompt>> SearchAsync(
        string? searchText = null,
        Guid? toolId = null,
        Guid? proyectoId = null,
        Guid? tagId = null,
        bool incluirInactivos = false,
        int skip = 0,
        int take = 50,
        CancellationToken ct = default)
    {
        var query = _context.Prompts
            .Include(p => p.Tool)
            .Include(p => p.CreatedByUser)
            .Include(p => p.Proyecto)
            .Include(p => p.PromptTags)
                .ThenInclude(pt => pt.Tag)
            .AsQueryable();

        if (!incluirInactivos)
            query = query.Where(p => p.Activo);

        if (!string.IsNullOrWhiteSpace(searchText))
        {
            var search = searchText.ToLower();
            query = query.Where(p =>
                p.Title.ToLower().Contains(search) ||
                p.Description.ToLower().Contains(search));
        }

        if (toolId.HasValue)
            query = query.Where(p => p.ToolId == toolId.Value);

        if (proyectoId.HasValue)
            query = query.Where(p => p.ProyectoId == proyectoId.Value);

        if (tagId.HasValue)
            query = query.Where(p => p.PromptTags.Any(pt => pt.TagId == tagId.Value));

        return await query
            .OrderByDescending(p => p.ModificadoEl)
            .Skip(skip)
            .Take(take)
            .ToListAsync(ct);
    }

    public async Task<Prompt> AddAsync(Prompt prompt, CancellationToken ct = default)
    {
        await _context.Prompts.AddAsync(prompt, ct);
        await _context.SaveChangesAsync(ct);
        return prompt;
    }

    public async Task UpdateAsync(Prompt prompt, CancellationToken ct = default)
    {
        prompt.ModificadoEl = DateTime.UtcNow;
        _context.Prompts.Update(prompt);
        await _context.SaveChangesAsync(ct);
    }

    public async Task DeleteAsync(Prompt prompt, CancellationToken ct = default)
    {
        _context.Prompts.Remove(prompt);
        await _context.SaveChangesAsync(ct);
    }

    public async Task<bool> ExistsAsync(Guid id, CancellationToken ct = default)
    {
        return await _context.Prompts.AnyAsync(x => x.Id == id, ct);
    }

    public async Task UpdateTagsAsync(Guid promptId, IEnumerable<Guid> tagIds, CancellationToken ct = default)
    {
        // Eliminar tags existentes
        var existingTags = await _context.PromptTags
            .Where(pt => pt.PromptId == promptId)
            .ToListAsync(ct);

        _context.PromptTags.RemoveRange(existingTags);

        // Agregar nuevos tags
        var newTags = tagIds.Select(tagId => new PromptTag
        {
            PromptId = promptId,
            TagId = tagId
        });

        await _context.PromptTags.AddRangeAsync(newTags, ct);
        await _context.SaveChangesAsync(ct);
    }
}
