using JSCHUB.Domain.Enums;

namespace JSCHUB.Domain.Entities;

/// <summary>
/// Recurso asociado a un proyecto (nota, enlace, referencia, documento, etc.)
/// </summary>
public class RecursoProyecto
{
    public Guid Id { get; set; }

    /// <summary>
    /// Proyecto al que pertenece el recurso
    /// </summary>
    public Guid ProyectoId { get; set; }

    /// <summary>
    /// Nombre del recurso (ej: "Guía despliegue", "Checklist release")
    /// </summary>
    public string Nombre { get; set; } = string.Empty;

    /// <summary>
    /// Tipo de recurso
    /// </summary>
    public TipoRecurso Tipo { get; set; } = TipoRecurso.Otro;

    /// <summary>
    /// URL del recurso (obligatorio si Tipo = Enlace o DocumentoExterno)
    /// </summary>
    public string? Url { get; set; }

    /// <summary>
    /// Contenido o notas (útil cuando Tipo = Nota)
    /// </summary>
    public string? Contenido { get; set; }

    /// <summary>
    /// Etiquetas para categorizar (separadas por coma)
    /// </summary>
    public string? Etiquetas { get; set; }

    // Auditoría
    public string CreadoPor { get; set; } = "sistema";
    public DateTime CreadoEl { get; set; } = DateTime.UtcNow;
    public string ModificadoPor { get; set; } = "sistema";
    public DateTime ModificadoEl { get; set; } = DateTime.UtcNow;

    // Navegación
    public Proyecto Proyecto { get; set; } = null!;

    /// <summary>
    /// Obtiene las etiquetas como lista
    /// </summary>
    public IEnumerable<string> ObtenerEtiquetas()
    {
        if (string.IsNullOrWhiteSpace(Etiquetas))
            return Enumerable.Empty<string>();

        return Etiquetas
            .Split(',', StringSplitOptions.RemoveEmptyEntries)
            .Select(e => e.Trim())
            .Where(e => !string.IsNullOrEmpty(e));
    }

    /// <summary>
    /// Establece las etiquetas desde una lista
    /// </summary>
    public void EstablecerEtiquetas(IEnumerable<string>? etiquetas)
    {
        if (etiquetas == null || !etiquetas.Any())
        {
            Etiquetas = null;
            return;
        }

        Etiquetas = string.Join(", ", etiquetas.Select(e => e.Trim()).Where(e => !string.IsNullOrEmpty(e)));
    }

    /// <summary>
    /// Indica si el recurso requiere URL según su tipo
    /// </summary>
    public bool RequiereUrl => Tipo == TipoRecurso.Enlace || Tipo == TipoRecurso.DocumentoExterno;

    /// <summary>
    /// Indica si el recurso requiere contenido según su tipo
    /// </summary>
    public bool RequiereContenido => Tipo == TipoRecurso.Nota;
}
