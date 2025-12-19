using JSCHUB.Domain.Enums;

namespace JSCHUB.Domain.Entities;

/// <summary>
/// Registro de un gasto de empresa. Preparado para futura funcionalidad de saldar deudas.
/// </summary>
public class Gasto
{
    public Guid Id { get; set; }
    
    /// <summary>Concepto o descripción del gasto (obligatorio)</summary>
    public string Concepto { get; set; } = string.Empty;
    
    /// <summary>Notas adicionales (opcional)</summary>
    public string? Notas { get; set; }
    
    /// <summary>Importe del gasto (debe ser > 0)</summary>
    public decimal Importe { get; set; }
    
    /// <summary>Código de moneda ISO (default: EUR)</summary>
    public string Moneda { get; set; } = "EUR";
    
    /// <summary>Id de la persona que pagó</summary>
    public Guid PagadoPorId { get; set; }
    
    /// <summary>Fecha del pago</summary>
    public DateOnly FechaPago { get; set; }
    
    /// <summary>Hora del pago</summary>
    public TimeOnly HoraPago { get; set; }
    
    /// <summary>Estado del gasto</summary>
    public EstadoGasto Estado { get; set; } = EstadoGasto.Registrado;
    
    // TODO: Adjuntos - implementar más adelante
    // public string? AdjuntoUrl { get; set; }
    
    // Navegación
    public Persona PagadoPor { get; set; } = null!;
}
