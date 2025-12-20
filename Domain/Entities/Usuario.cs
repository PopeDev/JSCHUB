namespace JSCHUB.Domain.Entities;

/// <summary>
/// Usuario del sistema que puede pagar gastos y ser asignado a tareas.
/// </summary>
public class Usuario
{
    public Guid Id { get; set; }

    /// <summary>Nombre del usuario (obligatorio)</summary>
    public string Nombre { get; set; } = string.Empty;

    /// <summary>Email de contacto (opcional)</summary>
    public string? Email { get; set; }

    /// <summary>Teléfono de contacto (opcional)</summary>
    public string? Telefono { get; set; }

    /// <summary>Si el usuario está activo</summary>
    public bool Activo { get; set; } = true;

    // Navegación
    public ICollection<Gasto> Gastos { get; set; } = [];
}
