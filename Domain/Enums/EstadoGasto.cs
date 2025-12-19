namespace JSCHUB.Domain.Enums;

/// <summary>
/// Estado de un gasto según su ciclo de vida
/// </summary>
public enum EstadoGasto
{
    /// <summary>No pagado, fecha de pago futura</summary>
    Previsto,

    /// <summary>No pagado, fecha de pago pasada</summary>
    Pendiente,

    /// <summary>Pagado</summary>
    Pagado,

    /// <summary>Pagado y saldado (el pagador pasa a ser la empresa/usuario)</summary>
    Saldado,

    /// <summary>Pagado en el pasado, pendiente de devolución</summary>
    PendienteDevolucion,

    /// <summary>Gasto anulado/cancelado (soft delete)</summary>
    Anulado
}
