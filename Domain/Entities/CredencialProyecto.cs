namespace JSCHUB.Domain.Entities;

/// <summary>
/// Credencial de acceso asociada a un enlace de proyecto.
/// La contraseña se almacena cifrada usando Data Protection API.
/// </summary>
public class CredencialProyecto
{
    public Guid Id { get; set; }

    /// <summary>
    /// Enlace del proyecto al que pertenece esta credencial
    /// </summary>
    public Guid EnlaceProyectoId { get; set; }

    /// <summary>
    /// Nombre descriptivo de la credencial (ej: "Admin", "FTP", "API Key")
    /// </summary>
    public string Nombre { get; set; } = string.Empty;

    /// <summary>
    /// Nombre de usuario o identificador de acceso
    /// </summary>
    public string Usuario { get; set; } = string.Empty;

    /// <summary>
    /// Contraseña cifrada con Data Protection API
    /// </summary>
    public string PasswordCifrado { get; set; } = string.Empty;

    /// <summary>
    /// Notas adicionales sobre la credencial
    /// </summary>
    public string? Notas { get; set; }

    /// <summary>
    /// Indica si la credencial está activa
    /// </summary>
    public bool Activa { get; set; } = true;

    /// <summary>
    /// Última vez que se usó/accedió a la credencial
    /// </summary>
    public DateTime? UltimoAcceso { get; set; }

    // Auditoría
    public string CreadoPor { get; set; } = "sistema";
    public DateTime CreadoEl { get; set; } = DateTime.UtcNow;
    public string ModificadoPor { get; set; } = "sistema";
    public DateTime ModificadoEl { get; set; } = DateTime.UtcNow;

    // Navegación
    public EnlaceProyecto EnlaceProyecto { get; set; } = null!;
}
