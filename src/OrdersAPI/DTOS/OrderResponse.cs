namespace OrdersAPI.DTOs;

/// <summary>
/// Objeto que representa una orden de compra generada.
/// </summary>
public record OrderResponse
{
    /// <summary>Identificador único de la orden.</summary>
    /// <example>f1e2d3c4-0000-0000-0000-aabbccddeeff</example>
    public required Guid Id { get; init; }

    /// <summary>Identificador del usuario dueño de la orden.</summary>
    /// <example>a1b2c3d4-0000-0000-0000-111122223333</example>
    public required Guid UsuarioId { get; init; }

    /// <summary>Monto total de la orden calculado a partir de sus ítems.</summary>
    /// <example>3000.00</example>
    public required decimal Total { get; init; }

    /// <summary>Estado actual de la orden.</summary>
    /// <example>Pendiente</example>
    public required string Estado { get; init; }

    /// <summary>Fecha y hora en que se creó la orden.</summary>
    /// <example>2024-03-10T11:00:00Z</example>
    public required DateTime FechaCreacion { get; init; }

    /// <summary>Lista con el detalle de los productos comprados.</summary>
    public required List<OrderItemResponse> Items { get; init; } = new();
}

/// <summary>
/// Detalle de un ítem dentro de la orden.
/// </summary>
public record OrderItemResponse
{
    /// <summary>Identificador del producto.</summary>
    /// <example>3fa85f64-5717-4562-b3fc-2c963f66afa6</example>
    public required Guid ProductoId { get; init; }

    /// <summary>Cantidad solicitada del producto.</summary>
    /// <example>2</example>
    public required int Cantidad { get; init; }

    /// <summary>Precio unitario del producto al momento de crear la orden.</summary>
    /// <example>1500.00</example>
    public required decimal PrecioUnitario { get; init; }
}
