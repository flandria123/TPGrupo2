using System.ComponentModel.DataAnnotations;

namespace OrdersAPI.DTOs;

/// <summary>
/// Objeto con el nuevo estado a asignar a la orden.
/// </summary>
public record UpdateStatusRequest
{
    /// <summary>Nuevo estado de la orden (Pendiente, Confirmada, Enviada, Entregada, Cancelada).</summary>
    /// <example>Confirmada</example>
    [Required(ErrorMessage = "El campo Estado es obligatorio.")]
    public required string Estado { get; init; }
}

