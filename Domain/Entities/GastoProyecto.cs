namespace JSCHUB.Domain.Entities;

/// <summary>
/// Tabla intermedia para la relación N:M entre Gasto y Proyecto.
/// Un gasto puede estar asociado a uno o varios proyectos.
/// Si está asociado SOLO al Proyecto General, se considera un gasto "General".
/// </summary>
public class GastoProyecto
{
    public Guid GastoId { get; set; }
    public Guid ProyectoId { get; set; }

    // Navegación
    public Gasto Gasto { get; set; } = null!;
    public Proyecto Proyecto { get; set; } = null!;
}
