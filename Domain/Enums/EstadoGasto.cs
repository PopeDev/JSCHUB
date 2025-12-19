namespace JSCHUB.Domain.Enums;

/// <summary>
/// Estado de un gasto para control y futura funcionalidad de saldar cuentas
/// </summary>
public enum EstadoGasto
{
    /// <summary>Gasto registrado y pendiente de saldar</summary>
    Registrado,
    
    /// <summary>Gasto anulado/cancelado (soft delete)</summary>
    Anulado,
    
    /// <summary>Gasto ya saldado entre las partes</summary>
    Saldado,
    
    /// <summary>Pendiente de devolución</summary>
    PendienteDevolucion
}
