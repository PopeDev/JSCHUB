namespace JSCHUB.Domain.Entities;

/// <summary>
/// Herramienta de IA a la que pertenece un prompt (ej: ChatGPT, Claude, Suno)
/// </summary>
public class Tool
{
    public Guid Id { get; set; }

    /// <summary>Nombre de la herramienta (obligatorio)</summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>Si la herramienta está activa</summary>
    public bool Activo { get; set; } = true;

    // Auditoría
    public DateTime CreadoEl { get; set; } = DateTime.UtcNow;

    // Navegación
    public ICollection<Prompt> Prompts { get; set; } = [];
}
