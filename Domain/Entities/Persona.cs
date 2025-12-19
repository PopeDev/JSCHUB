namespace JSCHUB.Domain.Entities;

/// <summary>
/// Persona que puede pagar gastos. Catálogo simple para vincular gastos.
/// </summary>
public class Persona
{
    public Guid Id { get; set; }
    
    /// <summary>Nombre de la persona (obligatorio)</summary>
    public string Nombre { get; set; } = string.Empty;
    
    /// <summary>Email de contacto (opcional)</summary>
    public string? Email { get; set; }
    
    /// <summary>Teléfono de contacto (opcional)</summary>
    public string? Telefono { get; set; }
    
    /// <summary>Si la persona está activa para asignar gastos</summary>
    public bool Activo { get; set; } = true;
    
    // Navegación
    public ICollection<Gasto> Gastos { get; set; } = [];
}
