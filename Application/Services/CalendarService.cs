using System.Globalization;
using JSCHUB.Application.DTOs;
using JSCHUB.Application.Interfaces;
using JSCHUB.Domain.Entities;
using JSCHUB.Domain.Interfaces;
using Microsoft.Extensions.Logging;

namespace JSCHUB.Application.Services;

public class CalendarService : ICalendarService
{
    private readonly IEventRepository _repository;
    private readonly ILogger<CalendarService> _logger;

    public CalendarService(IEventRepository repository, ILogger<CalendarService> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    public async Task<CalendarMonthDto> GetMonthAsync(int year, int month, CancellationToken ct = default)
    {
        // Calcular el primer día del mes
        var firstDayOfMonth = new DateTime(year, month, 1, 0, 0, 0, DateTimeKind.Utc);
        var lastDayOfMonth = firstDayOfMonth.AddMonths(1).AddDays(-1);

        // Calcular el primer día a mostrar (lunes de la semana que contiene el día 1)
        var firstDayOfWeek = firstDayOfMonth;
        while (firstDayOfWeek.DayOfWeek != DayOfWeek.Monday)
        {
            firstDayOfWeek = firstDayOfWeek.AddDays(-1);
        }

        // Calcular el último día a mostrar (completar 6 semanas = 42 días)
        var lastDayOfGrid = firstDayOfWeek.AddDays(41);

        // Obtener eventos del rango
        var events = await _repository.GetByRangeAsync(
            firstDayOfWeek,
            lastDayOfGrid.AddDays(1), // +1 para incluir eventos que empiezan ese día
            ct);

        var eventsList = events.ToList();

        // Construir los días del calendario
        var days = new List<CalendarDayDto>();
        var today = DateTime.UtcNow.Date;

        for (int i = 0; i < 42; i++)
        {
            var currentDate = firstDayOfWeek.AddDays(i);
            var isCurrentMonth = currentDate.Month == month;
            var isToday = currentDate.Date == today;

            // Filtrar eventos para este día
            var dayEvents = eventsList
                .Where(e => e.StartUtc.Date == currentDate.Date)
                .OrderBy(e => e.StartUtc)
                .Select(e => new CalendarItemDto(
                    e.Id,
                    e.Title,
                    e.StartUtc,
                    e.EndUtc,
                    e.MeetingUrl,
                    !string.IsNullOrEmpty(e.MeetingUrl)))
                .ToList();

            days.Add(new CalendarDayDto(
                currentDate,
                isCurrentMonth,
                isToday,
                dayEvents));
        }

        // Nombre del mes en español
        var culture = new CultureInfo("es-ES");
        var monthName = culture.DateTimeFormat.GetMonthName(month);
        monthName = char.ToUpper(monthName[0]) + monthName[1..];

        _logger.LogDebug("Calendario cargado: {Mes} {Anio} con {TotalEventos} eventos",
            monthName, year, eventsList.Count);

        return new CalendarMonthDto(year, month, monthName, days);
    }

    public async Task<EventDto?> GetByIdAsync(Guid id, CancellationToken ct = default)
    {
        var evento = await _repository.GetByIdAsync(id, ct);
        return evento is null ? null : MapToDto(evento);
    }

    public async Task<EventDto> CreateAsync(CreateEventDto dto, string usuario, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(dto.Title))
            throw new ArgumentException("El título del evento es obligatorio");

        if (dto.EndUtc <= dto.StartUtc)
            throw new ArgumentException("La fecha de fin debe ser posterior a la de inicio");

        var evento = new Event
        {
            Id = Guid.NewGuid(),
            Title = dto.Title.Trim(),
            StartUtc = dto.StartUtc,
            EndUtc = dto.EndUtc,
            MeetingUrl = string.IsNullOrWhiteSpace(dto.MeetingUrl) ? null : dto.MeetingUrl.Trim(),
            CreadoPor = usuario,
            CreadoEl = DateTime.UtcNow,
            ModificadoPor = usuario,
            ModificadoEl = DateTime.UtcNow
        };

        await _repository.AddAsync(evento, ct);

        _logger.LogInformation("Evento creado: {Id} - {Title} para {Start} por {Usuario}",
            evento.Id, evento.Title, evento.StartUtc, usuario);

        return MapToDto(evento);
    }

    public async Task<EventDto> UpdateAsync(Guid id, UpdateEventDto dto, string usuario, CancellationToken ct = default)
    {
        var evento = await _repository.GetByIdAsync(id, ct)
            ?? throw new InvalidOperationException($"No se encontró el evento con Id {id}");

        if (string.IsNullOrWhiteSpace(dto.Title))
            throw new ArgumentException("El título del evento es obligatorio");

        if (dto.EndUtc <= dto.StartUtc)
            throw new ArgumentException("La fecha de fin debe ser posterior a la de inicio");

        evento.Title = dto.Title.Trim();
        evento.StartUtc = dto.StartUtc;
        evento.EndUtc = dto.EndUtc;
        evento.MeetingUrl = string.IsNullOrWhiteSpace(dto.MeetingUrl) ? null : dto.MeetingUrl.Trim();
        evento.ModificadoPor = usuario;
        evento.ModificadoEl = DateTime.UtcNow;

        await _repository.UpdateAsync(evento, ct);

        _logger.LogInformation("Evento actualizado: {Id} - {Title} por {Usuario}",
            evento.Id, evento.Title, usuario);

        return MapToDto(evento);
    }

    public async Task DeleteAsync(Guid id, CancellationToken ct = default)
    {
        await _repository.DeleteAsync(id, ct);
        _logger.LogInformation("Evento eliminado: {Id}", id);
    }

    private static EventDto MapToDto(Event e) => new(
        e.Id,
        e.Title,
        e.StartUtc,
        e.EndUtc,
        e.MeetingUrl,
        e.NotifiedAtUtc,
        e.CreadoPor,
        e.CreadoEl,
        e.ModificadoPor,
        e.ModificadoEl
    );
}
