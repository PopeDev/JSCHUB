namespace JSCHUB.Domain.Entities;

/// <summary>
/// Prompt guardado para usar con herramientas de IA
/// </summary>
public class Prompt
{
    public Guid Id { get; set; }

    /// <summary>Título del prompt (obligatorio)</summary>
    public string Title { get; set; } = string.Empty;

    /// <summary>Descripción/contenido del prompt (obligatorio)</summary>
    public string Description { get; set; } = string.Empty;

    /// <summary>Si el prompt está activo</summary>
    public bool Activo { get; set; } = true;

    // Relación con Proyecto (opcional)
    public Guid? ProyectoId { get; set; }
    public Proyecto? Proyecto { get; set; }

    // Relación con Usuario que lo creó (obligatorio)
    public Guid CreatedByUserId { get; set; }
    public Usuario CreatedByUser { get; set; } = null!;

    // Relación con Tool (obligatorio)
    public Guid ToolId { get; set; }
    public Tool Tool { get; set; } = null!;

    // Auditoría
    public DateTime CreadoEl { get; set; } = DateTime.UtcNow;
    public DateTime ModificadoEl { get; set; } = DateTime.UtcNow;

    // Relaciones N:M con Tags
    public ICollection<PromptTag> PromptTags { get; set; } = [];

    /// <summary>
    /// Obtiene los tags asociados
    /// </summary>
    public IEnumerable<Tag> ObtenerTags()
    {
        return PromptTags.Select(pt => pt.Tag);
    }
}
