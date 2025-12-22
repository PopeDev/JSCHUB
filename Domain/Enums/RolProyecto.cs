namespace JSCHUB.Domain.Enums;

/// <summary>
/// Rol de un usuario dentro de un proyecto
/// </summary>
public enum RolProyecto
{
    /// <summary>
    /// Administrador del proyecto: acceso total (editar, eliminar, gestionar usuarios)
    /// </summary>
    Admin,

    /// <summary>
    /// Miembro del proyecto: puede crear, editar y eliminar sus propios items
    /// </summary>
    Miembro,

    /// <summary>
    /// Solo lectura: puede ver pero no crear ni editar
    /// </summary>
    Viewer
}
