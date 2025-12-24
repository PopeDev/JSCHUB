using FluentValidation;
using JSCHUB.Application.DTOs;
using JSCHUB.Application.Interfaces;
using JSCHUB.Domain.Entities;
using JSCHUB.Domain.Enums;
using JSCHUB.Domain.Interfaces;
using Microsoft.Extensions.Logging;

namespace JSCHUB.Application.Services;

public class SprintService : ISprintService
{
    private readonly ISprintRepository _repository;
    private readonly IProyectoRepository _proyectoRepository;
    private readonly IKanbanRepository _kanbanRepository;
    private readonly IValidator<CreateSprintDto> _createValidator;
    private readonly IValidator<UpdateSprintDto> _updateValidator;
    private readonly ILogger<SprintService> _logger;

    public SprintService(
        ISprintRepository repository,
        IProyectoRepository proyectoRepository,
        IKanbanRepository kanbanRepository,
        IValidator<CreateSprintDto> createValidator,
        IValidator<UpdateSprintDto> updateValidator,
        ILogger<SprintService> logger)
    {
        _repository = repository;
        _proyectoRepository = proyectoRepository;
        _kanbanRepository = kanbanRepository;
        _createValidator = createValidator;
        _updateValidator = updateValidator;
        _logger = logger;
    }

    public async Task<SprintDto?> GetByIdAsync(Guid id, CancellationToken ct = default)
    {
        var sprint = await _repository.GetByIdAsync(id, ct);
        if (sprint == null) return null;

        var historico = await _repository.GetHistoricoAsync(id, ct);
        return MapToDto(sprint, historico);
    }

    public async Task<IEnumerable<SprintDto>> GetByProyectoIdAsync(Guid proyectoId, CancellationToken ct = default)
    {
        var sprints = await _repository.GetByProyectoIdAsync(proyectoId, ct);
        // Para listados no cargamos el histórico completo para optimizar
        return sprints.Select(s => MapToDto(s, []));
    }

    public async Task<SprintDto?> GetSprintActivoAsync(Guid proyectoId, CancellationToken ct = default)
    {
        var sprint = await _repository.GetSprintActivoAsync(proyectoId, ct);
        return sprint == null ? null : MapToDto(sprint, []);
    }

    public async Task<SprintDto> CreateAsync(CreateSprintDto dto, string usuario, CancellationToken ct = default)
    {
        await _createValidator.ValidateAndThrowAsync(dto, ct);

        // Verificar que el proyecto existe
        var proyecto = await _proyectoRepository.GetByIdAsync(dto.ProyectoId, ct)
            ?? throw new KeyNotFoundException($"Proyecto {dto.ProyectoId} no encontrado");

        var sprint = new Sprint
        {
            Id = Guid.NewGuid(),
            ProyectoId = dto.ProyectoId,
            Nombre = dto.Nombre,
            Temporizacion = dto.Temporizacion,
            Objetivo = dto.Objetivo,
            FechaInicio = dto.FechaInicio,
            FechaFin = dto.FechaFin,
            Estado = EstadoSprint.Pendiente,
            CreadoPor = usuario,
            CreadoEl = DateTime.UtcNow,
            ModificadoPor = usuario,
            ModificadoEl = DateTime.UtcNow
        };

        var created = await _repository.AddAsync(sprint, ct);
        _logger.LogInformation("Sprint creado: {Id} - {Nombre}", created.Id, created.Nombre);

        return MapToDto(created, []);
    }

    public async Task<SprintDto> UpdateAsync(Guid id, UpdateSprintDto dto, string usuario, CancellationToken ct = default)
    {
        await _updateValidator.ValidateAndThrowAsync(dto, ct);

        var sprint = await _repository.GetByIdAsync(id, ct)
            ?? throw new KeyNotFoundException($"Sprint {id} no encontrado");

        if (sprint.Estado == EstadoSprint.Cerrado)
            throw new InvalidOperationException("No se puede editar un sprint cerrado");

        sprint.Nombre = dto.Nombre;
        sprint.Temporizacion = dto.Temporizacion;
        sprint.Objetivo = dto.Objetivo;
        sprint.FechaInicio = dto.FechaInicio;
        sprint.FechaFin = dto.FechaFin;
        sprint.ModificadoPor = usuario;
        sprint.ModificadoEl = DateTime.UtcNow;

        await _repository.UpdateAsync(sprint, ct);
        _logger.LogInformation("Sprint actualizado: {Id}", id);

        return MapToDto(sprint, []);
    }

    public async Task ActivarSprintAsync(Guid id, string usuario, CancellationToken ct = default)
    {
        var sprint = await _repository.GetByIdAsync(id, ct)
            ?? throw new KeyNotFoundException($"Sprint {id} no encontrado");

        if (sprint.Estado != EstadoSprint.Pendiente)
            throw new InvalidOperationException($"El sprint debe estar Pendiente para activarse (Estado actual: {sprint.Estado})");

        // Verificar si hay otro activo
        var activoActual = await _repository.GetSprintActivoAsync(sprint.ProyectoId, ct);
        if (activoActual != null)
            throw new InvalidOperationException($"El proyecto ya tiene un sprint activo: {activoActual.Nombre}. Ciérrelo antes de activar uno nuevo.");

        var proyecto = await _proyectoRepository.GetByIdAsync(sprint.ProyectoId, ct);

        // Activar sprint
        sprint.Estado = EstadoSprint.Activo;
        sprint.ModificadoPor = usuario;
        sprint.ModificadoEl = DateTime.UtcNow;

        // Asignar al proyecto
        proyecto.SprintActivoId = sprint.Id;
        proyecto.ModificadoPor = usuario;
        proyecto.ModificadoEl = DateTime.UtcNow;

        // Asociar tareas pendientes del backlog (sin sprint) o existentes
        // NOTA: Según lógica definida, al activar, las tareas del board que no estén en sprint se asocian.
        // Pero simplificaremos: al crear tareas ya se asignan si hay sprint activo.
        // Aquí vamos a marcar las tareas existentes en el board como "Comprometidas" para este sprint
        // NOTA: Reemplazo de GetBoardAsync por GetColumnsByProyectoIdWithTasksAsync para evitar error de repositorio
        // obtenemos las columnas directamente del repositorio
        var columnas = await _kanbanRepository.GetColumnsByProyectoIdAsync(sprint.ProyectoId, ct);
        var columnasOrdenadas = columnas.OrderBy(c => c.Posicion).ToList();
        
        // Asumimos que la última columna es "Finalizado", las tareas ahí no entran al nuevo sprint si ya estaban
        // Pero si el sprint empieza de cero, tomamos todas las tareas del board que NO estén en la última columna
        if (columnasOrdenadas.Any())
        {
            var ultimaColumnaId = columnasOrdenadas.Last().Id;
            // Usamos el metodo nuevo del repositorio
            var tareasParaSprint = await _kanbanRepository.GetTareasByProyectoAsync(sprint.ProyectoId, ct);
            
            int countComprometidas = 0;
            foreach (var tarea in tareasParaSprint.Where(t => t.ColumnaId != ultimaColumnaId))
            {
                tarea.SprintId = sprint.Id;
                tarea.EsComprometida = true; // Snapshot de inicio
                tarea.ModificadoPor = usuario;
                tarea.ModificadoEl = DateTime.UtcNow;
                
                // Si viene de otro sprint, ya tiene su contador. Si es nueva, null -> 0.
                // En realidad task.SprintsTranscurridos es int, por defecto 0.
                if (tarea.SprintsTranscurridos == 0) 
                    tarea.SprintsTranscurridos = 1; // Empieza su primer sprint
                else 
                    tarea.SprintsTranscurridos++; // Pasa al siguiente
                
                await _kanbanRepository.UpdateTaskAsync(tarea, ct);
                countComprometidas++;
            }
            
            sprint.TareasComprometidas = countComprometidas;
        }

        await _repository.UpdateAsync(sprint, ct);
        await _proyectoRepository.UpdateAsync(proyecto, ct);

        _logger.LogInformation("Sprint activado: {Id} - {Nombre}", id, sprint.Nombre);
    }

    public async Task<CierreSprintResultDto> CerrarSprintActivoAsync(Guid proyectoId, Guid? siguienteSprintId, string usuario, CancellationToken ct = default)
    {
        var sprint = await _repository.GetSprintActivoAsync(proyectoId, ct)
            ?? throw new InvalidOperationException("No hay sprint activo en este proyecto");

        var proyecto = await _proyectoRepository.GetByIdAsync(proyectoId, ct);
        var columnas = await _kanbanRepository.GetColumnsByProyectoIdAsync(proyectoId, ct);
        
        if (!columnas.Any())
            throw new InvalidOperationException("El tablero no tiene columnas");

        var ultimaColumna = columnas.OrderBy(c => c.Posicion).Last();
        var tareasEnSprint = await _kanbanRepository.GetTareasByProyectoAsync(proyectoId, ct);
        // Filtrar en memoria por SprintId
        tareasEnSprint = tareasEnSprint.Where(t => t.SprintId == sprint.Id).ToList();

        var historicos = new List<SprintTareaHistorico>();
        int tareasEntregadas = 0;
        int tareasMovidas = 0;

        foreach (var tarea in tareasEnSprint)
        {
            bool entregada = tarea.ColumnaId == ultimaColumna.Id;
            var nombreColumna = columnas.FirstOrDefault(c => c.Id == tarea.ColumnaId)?.Titulo;
            
            // Crear histórico
            historicos.Add(new SprintTareaHistorico
            {
                Id = Guid.NewGuid(),
                SprintId = sprint.Id,
                TareaId = tarea.Id,
                TareaTitulo = tarea.Titulo,
                TareaDescripcion = tarea.Descripcion,
                AsignadoANombre = tarea.AsignadoA?.Nombre,
                ColumnaFinal = nombreColumna,
                FueEntregada = entregada,
                EraComprometida = tarea.EsComprometida,
                SprintsTranscurridos = tarea.SprintsTranscurridos,
                FechaRegistro = DateTime.UtcNow
            });

            if (entregada)
            {
                // Tarea finalizada: Se queda en el board pero ya "histórica" para este sprint
                // En realidad, en Kanban puro las tareas finalizadas suelen archivarse o dejarse. 
                // Aquí las dejamos, pero les quitamos el SprintId para que no cuenten para el siguiente, 
                // O las dejamos con el SprintId del sprint cerrado (histórico).
                // Decisión: Las dejamos con SprintId = sprint.Id (que ahora estará cerrado)
                // Así sabemos en qué sprint se terminaron.
                if (tarea.EsComprometida) tareasEntregadas++;
            }
            else
            {
                // Tarea pendiente: Mover al siguiente sprint (si hay) o dejar sin sprint (backlog)
                tarea.SprintId = siguienteSprintId;
                tarea.EsComprometida = siguienteSprintId.HasValue; // Si pasa al siguiente, es comprometida del siguiente
                tarea.ModificadoPor = usuario;
                tarea.ModificadoEl = DateTime.UtcNow;
                
                // Incrementar contador de sprints si pasa a otro activo
                if (siguienteSprintId.HasValue)
                {
                    tarea.SprintsTranscurridos++;
                }
                
                await _kanbanRepository.UpdateTaskAsync(tarea, ct);
                tareasMovidas++;
            }
        }

        // Guardar históricos
        await _repository.AddHistoricoRangeAsync(historicos, ct);

        // Actualizar métricas del sprint cerrado
        sprint.Estado = EstadoSprint.Cerrado;
        sprint.TareasEntregadas = tareasEntregadas;
        sprint.FechaCierre = DateTime.UtcNow;
        
        // Evitar división por cero
        decimal completitud = 0;
        if (sprint.TareasComprometidas.HasValue && sprint.TareasComprometidas.Value > 0)
        {
            completitud = (decimal)tareasEntregadas / sprint.TareasComprometidas.Value * 100;
        }
        else if (tareasEntregadas > 0)
        {
            // Si no había comprometidas (raro) pero entregamos cosas, 100%? o cálculo sobre total?
            // Ajustamos a lógica de negocio: sobre comprometidas.
            completitud = 100; 
        }
        sprint.PorcentajeCompletitud = completitud;
        sprint.ModificadoPor = usuario;
        sprint.ModificadoEl = DateTime.UtcNow;

        await _repository.UpdateAsync(sprint, ct);

        // Actualizar proyecto
        proyecto.SprintActivoId = siguienteSprintId;
        proyecto.ModificadoPor = usuario;
        proyecto.ModificadoEl = DateTime.UtcNow;
        await _proyectoRepository.UpdateAsync(proyecto, ct);

        // Si hay siguiente sprint, activarlo
        if (siguienteSprintId.HasValue)
        {
            var nextSprint = await _repository.GetByIdAsync(siguienteSprintId.Value, ct);
            if (nextSprint != null && nextSprint.Estado == EstadoSprint.Pendiente)
            {
                nextSprint.Estado = EstadoSprint.Activo;
                nextSprint.TareasComprometidas = tareasMovidas; // Las que pasaron son las primeras comprometidas
                nextSprint.ModificadoPor = usuario;
                nextSprint.ModificadoEl = DateTime.UtcNow;
                await _repository.UpdateAsync(nextSprint, ct);
            }
        }

        _logger.LogInformation("Sprint cerrado: {Id}. Completitud: {Completitud}%", sprint.Id, completitud);

        return new CierreSprintResultDto(sprint.Id, siguienteSprintId, tareasMovidas, tareasEntregadas, completitud);
    }

    public async Task DeleteAsync(Guid id, CancellationToken ct = default)
    {
        var sprint = await _repository.GetByIdAsync(id, ct)
            ?? throw new KeyNotFoundException($"Sprint {id} no encontrado");

        if (sprint.Estado != EstadoSprint.Pendiente)
            throw new InvalidOperationException("Solo se pueden eliminar sprints en estado Pendiente");

        await _repository.DeleteAsync(sprint, ct);
        _logger.LogInformation("Sprint eliminado: {Id}", id);
    }

    private static SprintDto MapToDto(Sprint sprint, IEnumerable<SprintTareaHistorico> historico) => new(
        sprint.Id,
        sprint.ProyectoId,
        sprint.Nombre,
        sprint.Temporizacion,
        sprint.Objetivo,
        sprint.FechaInicio,
        sprint.FechaFin,
        sprint.DuracionDias,
        sprint.Estado,
        sprint.TareasComprometidas,
        sprint.TareasEntregadas,
        sprint.PorcentajeCompletitud,
        sprint.FechaCierre,
        sprint.CreadoPor,
        sprint.CreadoEl,
        historico.Select(h => new SprintTareaHistoricoDto(
            h.Id,
            h.TareaId,
            h.TareaTitulo,
            h.TareaDescripcion,
            h.AsignadoANombre,
            h.FueEntregada,
            h.ColumnaFinal,
            h.EraComprometida,
            h.SprintsTranscurridos,
            h.FechaRegistro
        ))
    );
}
