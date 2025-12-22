using JSCHUB.Domain.Enums;

namespace JSCHUB.Domain.Entities;

/// <summary>
/// Tabla intermedia para la relación N:M entre Usuario y Proyecto.
/// Define qué usuarios tienen acceso a qué proyectos y con qué rol.
/// </summary>
public class UsuarioProyecto
{
    public Guid UsuarioId { get; set; }
    public Guid ProyectoId { get; set; }

    /// <summary>
    /// Rol del usuario en el proyecto (Admin, Miembro, Viewer)
    /// </summary>
    public RolProyecto Rol { get; set; } = RolProyecto.Miembro;

    /// <summary>
    /// Fecha en que se asignó el usuario al proyecto
    /// </summary>
    public DateTime FechaAsignacion { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Usuario que realizó la asignación
    /// </summary>
    public string AsignadoPor { get; set; } = "sistema";

    // Navegación
    public Usuario Usuario { get; set; } = null!;
    public Proyecto Proyecto { get; set; } = null!;
}
