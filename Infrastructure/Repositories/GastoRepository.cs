using JSCHUB.Domain.Entities;
using JSCHUB.Domain.Enums;
using JSCHUB.Domain.Interfaces;
using Microsoft.EntityFrameworkCore;
using JSCHUB.Infrastructure.Data;

namespace JSCHUB.Infrastructure.Repositories;

public class GastoRepository : IGastoRepository
{
    private readonly ReminderDbContext _context;
    private Guid? _proyectoGeneralIdCache;

    public GastoRepository(ReminderDbContext context)
    {
        _context = context;
    }

    public async Task<Gasto?> GetByIdAsync(Guid id, CancellationToken ct = default)
    {
        return await _context.Gastos.FindAsync([id], ct);
    }

    public async Task<Gasto?> GetByIdWithRelacionesAsync(Guid id, CancellationToken ct = default)
    {
        return await _context.Gastos
            .Include(x => x.PagadoPor)
            .Include(x => x.GastosProyecto)
                .ThenInclude(gp => gp.Proyecto)
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
        Guid? proyectoId = null,
        IEnumerable<Guid>? usuarioProyectoIds = null,
        int skip = 0,
        int take = 50,
        CancellationToken ct = default)
    {
        var query = await BuildQueryAsync(searchText, pagadoPorId, fechaDesde, fechaHasta,
            importeMin, importeMax, estado, proyectoId, usuarioProyectoIds, ct);

        return await query
            .Include(x => x.PagadoPor)
            .Include(x => x.GastosProyecto)
                .ThenInclude(gp => gp.Proyecto)
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
        Guid? proyectoId = null,
        IEnumerable<Guid>? usuarioProyectoIds = null,
        CancellationToken ct = default)
    {
        var query = await BuildQueryAsync(searchText, pagadoPorId, fechaDesde, fechaHasta,
            importeMin, importeMax, estado, proyectoId, usuarioProyectoIds, ct);
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

    public async Task SetProyectosAsync(Guid gastoId, IEnumerable<Guid> proyectoIds, CancellationToken ct = default)
    {
        // Eliminar asociaciones existentes
        var existentes = await _context.GastosProyectos
            .Where(gp => gp.GastoId == gastoId)
            .ToListAsync(ct);

        _context.GastosProyectos.RemoveRange(existentes);

        // Agregar nuevas asociaciones
        var nuevas = proyectoIds.Select(pId => new GastoProyecto
        {
            GastoId = gastoId,
            ProyectoId = pId
        });

        await _context.GastosProyectos.AddRangeAsync(nuevas, ct);
        await _context.SaveChangesAsync(ct);
    }

    public async Task<Guid?> GetProyectoGeneralIdAsync(CancellationToken ct = default)
    {
        if (_proyectoGeneralIdCache.HasValue)
            return _proyectoGeneralIdCache;

        var proyecto = await _context.Proyectos
            .FirstOrDefaultAsync(p => p.EsGeneral, ct);

        _proyectoGeneralIdCache = proyecto?.Id;
        return _proyectoGeneralIdCache;
    }

    private async Task<IQueryable<Gasto>> BuildQueryAsync(
        string? searchText,
        Guid? pagadoPorId,
        DateOnly? fechaDesde,
        DateOnly? fechaHasta,
        decimal? importeMin,
        decimal? importeMax,
        EstadoGasto? estado,
        Guid? proyectoId,
        IEnumerable<Guid>? usuarioProyectoIds,
        CancellationToken ct)
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

        // Filtrado por proyecto
        if (proyectoId.HasValue)
        {
            // Si se filtra por un proyecto específico, incluir también gastos del Proyecto General
            var proyectoGeneralId = await GetProyectoGeneralIdAsync(ct);

            if (proyectoGeneralId.HasValue && proyectoId.Value != proyectoGeneralId.Value)
            {
                // Mostrar gastos del proyecto seleccionado O del Proyecto General
                query = query.Where(x =>
                    x.GastosProyecto.Any(gp => gp.ProyectoId == proyectoId.Value) ||
                    x.GastosProyecto.Any(gp => gp.ProyectoId == proyectoGeneralId.Value));
            }
            else
            {
                // Si es el Proyecto General, mostrar solo gastos del Proyecto General
                query = query.Where(x =>
                    x.GastosProyecto.Any(gp => gp.ProyectoId == proyectoId.Value));
            }
        }
        else if (usuarioProyectoIds != null && usuarioProyectoIds.Any())
        {
            // Mostrar gastos de todos los proyectos a los que el usuario tiene acceso
            var proyectoIdsList = usuarioProyectoIds.ToList();
            query = query.Where(x =>
                x.GastosProyecto.Any(gp => proyectoIdsList.Contains(gp.ProyectoId)));
        }

        return query;
    }
}
