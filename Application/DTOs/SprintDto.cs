using JSCHUB.Domain.Enums;

namespace JSCHUB.Application.DTOs;

/// <summary>
/// DTO de lectura para un Sprint
/// </summary>
public record SprintDto(
    Guid Id,
    Guid ProyectoId,
    string Nombre,
    string Temporizacion,
    string? Objetivo,
    DateOnly FechaInicio,
    DateOnly FechaFin,
    int DuracionDias,
    EstadoSprint Estado,
    int? TareasComprometidas,
    int? TareasEntregadas,
    decimal? PorcentajeCompletitud,
    DateTime? FechaCierre,
    string CreadoPor,
    DateTime CreadoEl,
    IEnumerable<SprintTareaHistoricoDto> Historico
);

/// <summary>
/// DTO de lectura para histórico de tarea de sprint
/// </summary>
public record SprintTareaHistoricoDto(
    Guid Id,
    Guid TareaId,
    string TareaTitulo,
    string? TareaDescripcion,
    string? AsignadoANombre,
    bool FueEntregada,
    string? ColumnaFinal,
    bool EraComprometida,
    int SprintsTranscurridos,
    DateTime FechaRegistro
);

/// <summary>
/// DTO para crear un sprint
/// </summary>
public record CreateSprintDto(
    Guid ProyectoId,
    string Nombre,
    string Temporizacion,
    string? Objetivo,
    DateOnly FechaInicio,
    DateOnly FechaFin
);

/// <summary>
/// DTO para actualizar un sprint
/// </summary>
public class UpdateSprintDto
{
    public string Nombre { get; set; } = string.Empty;
    public string Temporizacion { get; set; } = string.Empty;
    public string? Objetivo { get; set; }
    public DateOnly FechaInicio { get; set; }
    public DateOnly FechaFin { get; set; }

    public UpdateSprintDto() { }

    public UpdateSprintDto(string nombre, string temporizacion, string? objetivo, DateOnly fechaInicio, DateOnly fechaFin)
    {
        Nombre = nombre;
        Temporizacion = temporizacion;
        Objetivo = objetivo;
        FechaInicio = fechaInicio;
        FechaFin = fechaFin;
    }
}

/// <summary>
/// DTO con resultado del cierre del sprint
/// </summary>
public record CierreSprintResultDto(
    Guid SprintCerradoId,
    Guid? NuevoSprintActivoId,
    int TareasMovidas,
    int TareasEntregadas,
    decimal PorcentajeCompletitud
);
