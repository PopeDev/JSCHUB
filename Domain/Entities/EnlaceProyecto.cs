using JSCHUB.Domain.Enums;

namespace JSCHUB.Domain.Entities;

/// <summary>
/// Enlace de interés asociado a un proyecto
/// </summary>
public class EnlaceProyecto
{
    public Guid Id { get; set; }

    /// <summary>
    /// Proyecto al que pertenece el enlace
    /// </summary>
    public Guid ProyectoId { get; set; }

    /// <summary>
    /// Título del enlace (ej: "Repositorio", "Producción")
    /// </summary>
    public string Titulo { get; set; } = string.Empty;

    /// <summary>
    /// URL del enlace (obligatorio)
    /// </summary>
    public string Url { get; set; } = string.Empty;

    /// <summary>
    /// Descripción adicional del enlace
    /// </summary>
    public string? Descripcion { get; set; }

    /// <summary>
    /// Tipo de enlace para filtrado
    /// </summary>
    public TipoEnlace Tipo { get; set; } = TipoEnlace.Otro;

    /// <summary>
    /// Orden para mostrar (menor = más arriba)
    /// </summary>
    public int Orden { get; set; } = 0;

    // Auditoría
    public string CreadoPor { get; set; } = "sistema";
    public DateTime CreadoEl { get; set; } = DateTime.UtcNow;
    public string ModificadoPor { get; set; } = "sistema";
    public DateTime ModificadoEl { get; set; } = DateTime.UtcNow;

    // Navegación
    public Proyecto Proyecto { get; set; } = null!;
}
