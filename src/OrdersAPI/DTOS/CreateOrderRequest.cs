using System.ComponentModel.DataAnnotations;

namespace OrdersAPI.DTOs;

/// <summary>
/// Objeto con los datos para crear una nueva orden de compra.
/// </summary>
public record CreateOrderRequest
{
    /// <summary>Identificador del usuario que realiza la orden.</summary>
    /// <example>21b75cee-f8f6-4261-a370-21b16c40967e</example>
    [Required(ErrorMessage = "El UsuarioId es obligatorio.")]
    public required Guid UsuarioId { get; init; }

    /// <summary>Lista de productos a incluir en la orden.</summary>
    [Required(ErrorMessage = "La lista de items es obligatoria.")]
    [MinLength(1, ErrorMessage = "La orden debe contener al menos un item.")]
    public required List<OrderItemRequest> Items { get; init; } = new();
}

/// <summary>
/// Detalle de un producto y su cantidad para la orden.
/// </summary>
public record OrderItemRequest
{
    /// <summary>Identificador del producto.</summary>
    /// <example>3fa85f64-5717-4562-b3fc-2c963f66afa6</example>
    [Required(ErrorMessage = "El ProductoId es obligatorio.")]
    public required Guid ProductoId { get; init; }

    /// <summary>Cantidad solicitada del producto.</summary>
    /// <example>2</example>
    [Required(ErrorMessage = "La cantidad es obligatoria.")]
    [Range(1, int.MaxValue, ErrorMessage = "La cantidad debe ser mayor a cero.")]
    public required int Cantidad { get; init; }
}
