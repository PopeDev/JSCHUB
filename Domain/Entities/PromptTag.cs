namespace JSCHUB.Domain.Entities;

/// <summary>
/// Tabla intermedia para la relación N:M entre Prompt y Tag
/// </summary>
public class PromptTag
{
    public Guid PromptId { get; set; }
    public Prompt Prompt { get; set; } = null!;

    public Guid TagId { get; set; }
    public Tag Tag { get; set; } = null!;
}
