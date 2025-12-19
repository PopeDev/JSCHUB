using JSCHUB.Domain.Entities;
using JSCHUB.Domain.Enums;
using JSCHUB.Domain.Interfaces;
using Microsoft.EntityFrameworkCore;
using JSCHUB.Infrastructure.Data;

namespace JSCHUB.Infrastructure.Repositories;

public class GastoRepository : IGastoRepository
{
    private readonly ReminderDbContext _context;

    public GastoRepository(ReminderDbContext context)
    {
        _context = context;
    }

    public async Task<Gasto?> GetByIdAsync(Guid id, CancellationToken ct = default)
    {
        return await _context.Gastos.FindAsync([id], ct);
    }

    public async Task<Gasto?> GetByIdWithUsuarioAsync(Guid id, CancellationToken ct = default)
    {
        return await _context.Gastos
            .Include(x => x.PagadoPor)
            .FirstOrDefaultAsync(x => x.Id == id, ct);
    }

    public async Task<IEnumerable<Gasto>> SearchAsync(
        string? searchText = null,
        Guid? pagadoPorId = null,
        DateOnly? fechaDesde = null,
        DateOnly? fechaHasta = null,
        decimal? importeMin = null,
        decimal? importeMax = null,
        EstadoGasto? estado = null,
        int skip = 0,
        int take = 50,
        CancellationToken ct = default)
    {
        var query = BuildQuery(searchText, pagadoPorId, fechaDesde, fechaHasta, importeMin, importeMax, estado);
        
        return await query
            .Include(x => x.PagadoPor)
            .OrderByDescending(x => x.FechaPago)
            .ThenByDescending(x => x.HoraPago)
            .Skip(skip)
            .Take(take)
            .ToListAsync(ct);
    }

    public async Task<int> CountAsync(
        string? searchText = null,
        Guid? pagadoPorId = null,
        DateOnly? fechaDesde = null,
        DateOnly? fechaHasta = null,
        decimal? importeMin = null,
        decimal? importeMax = null,
        EstadoGasto? estado = null,
        CancellationToken ct = default)
    {
        var query = BuildQuery(searchText, pagadoPorId, fechaDesde, fechaHasta, importeMin, importeMax, estado);
        return await query.CountAsync(ct);
    }

    public async Task<Gasto> AddAsync(Gasto gasto, CancellationToken ct = default)
    {
        await _context.Gastos.AddAsync(gasto, ct);
        await _context.SaveChangesAsync(ct);
        return gasto;
    }

    public async Task UpdateAsync(Gasto gasto, CancellationToken ct = default)
    {
        _context.Gastos.Update(gasto);
        await _context.SaveChangesAsync(ct);
    }

    public async Task DeleteAsync(Gasto gasto, CancellationToken ct = default)
    {
        _context.Gastos.Remove(gasto);
        await _context.SaveChangesAsync(ct);
    }

    private IQueryable<Gasto> BuildQuery(
        string? searchText,
        Guid? pagadoPorId,
        DateOnly? fechaDesde,
        DateOnly? fechaHasta,
        decimal? importeMin,
        decimal? importeMax,
        EstadoGasto? estado)
    {
        var query = _context.Gastos.AsQueryable();

        if (!string.IsNullOrWhiteSpace(searchText))
        {
            var search = searchText.ToLower();
            query = query.Where(x => 
                x.Concepto.ToLower().Contains(search) || 
                (x.Notas != null && x.Notas.ToLower().Contains(search)));
        }

        if (pagadoPorId.HasValue)
        {
            query = query.Where(x => x.PagadoPorId == pagadoPorId.Value);
        }

        if (fechaDesde.HasValue)
        {
            query = query.Where(x => x.FechaPago >= fechaDesde.Value);
        }

        if (fechaHasta.HasValue)
        {
            query = query.Where(x => x.FechaPago <= fechaHasta.Value);
        }

        if (importeMin.HasValue)
        {
            query = query.Where(x => x.Importe >= importeMin.Value);
        }

        if (importeMax.HasValue)
        {
            query = query.Where(x => x.Importe <= importeMax.Value);
        }

        if (estado.HasValue)
        {
            query = query.Where(x => x.Estado == estado.Value);
        }

        return query;
    }
}
