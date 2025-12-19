namespace JSCHUB.Domain.Enums;

/// <summary>
/// Tipo de enlace de proyecto
/// </summary>
public enum TipoEnlace
{
    /// <summary>
    /// Repositorio de código
    /// </summary>
    Repositorio,

    /// <summary>
    /// Documentación
    /// </summary>
    Documentacion,

    /// <summary>
    /// Panel de administración o backoffice
    /// </summary>
    Panel,

    /// <summary>
    /// Entorno de producción o preproducción
    /// </summary>
    Entorno,

    /// <summary>
    /// Diseño (Figma, Sketch, etc.)
    /// </summary>
    Diseno,

    /// <summary>
    /// Otro tipo de enlace
    /// </summary>
    Otro
}
