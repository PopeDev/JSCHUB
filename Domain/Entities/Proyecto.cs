using JSCHUB.Domain.Enums;

namespace JSCHUB.Domain.Entities;

/// <summary>
/// Representa un proyecto de la empresa (cliente, interno, producto, etc.)
/// </summary>
public class Proyecto
{
    public Guid Id { get; set; }

    /// <summary>
    /// Nombre del proyecto (obligatorio)
    /// </summary>
    public string Nombre { get; set; } = string.Empty;

    /// <summary>
    /// Descripción del proyecto (opcional)
    /// </summary>
    public string? Descripcion { get; set; }

    /// <summary>
    /// Estado actual del proyecto
    /// </summary>
    public EstadoProyecto Estado { get; set; } = EstadoProyecto.Activo;

    /// <summary>
    /// Enlace principal destacado (repo, documentación, panel cliente)
    /// </summary>
    public string? EnlacePrincipal { get; set; }

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
    public ICollection<EnlaceProyecto> Enlaces { get; set; } = new List<EnlaceProyecto>();
    public ICollection<RecursoProyecto> Recursos { get; set; } = new List<RecursoProyecto>();

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
    /// Archiva el proyecto
    /// </summary>
    public void Archivar(string usuario)
    {
        Estado = EstadoProyecto.Archivado;
        ModificadoPor = usuario;
        ModificadoEl = DateTime.UtcNow;
    }

    /// <summary>
    /// Reactiva un proyecto archivado
    /// </summary>
    public void Reactivar(string usuario)
    {
        Estado = EstadoProyecto.Activo;
        ModificadoPor = usuario;
        ModificadoEl = DateTime.UtcNow;
    }
}
