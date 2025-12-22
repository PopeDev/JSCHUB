namespace JSCHUB.Domain.Entities;

/// <summary>
/// Tabla intermedia para la relación N:M entre ReminderItem y Proyecto.
/// Un recordatorio puede estar asociado a uno o varios proyectos.
/// Si está asociado SOLO al Proyecto General, se considera un recordatorio "General".
/// </summary>
public class ReminderItemProyecto
{
    public Guid ReminderItemId { get; set; }
    public Guid ProyectoId { get; set; }

    // Navegación
    public ReminderItem ReminderItem { get; set; } = null!;
    public Proyecto Proyecto { get; set; } = null!;
}
