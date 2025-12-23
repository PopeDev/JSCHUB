namespace JSCHUB.Domain.Entities;

/// <summary>
/// Etiqueta para categorizar prompts (ej: pop art, blanco y negro, vintage, música)
/// </summary>
public class Tag
{
    public Guid Id { get; set; }

    /// <summary>Nombre de la etiqueta (obligatorio)</summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>Si la etiqueta está activa</summary>
    public bool Activo { get; set; } = true;

    // Auditoría
    public DateTime CreadoEl { get; set; } = DateTime.UtcNow;

    // Relaciones N:M
    public ICollection<PromptTag> PromptTags { get; set; } = [];
}
