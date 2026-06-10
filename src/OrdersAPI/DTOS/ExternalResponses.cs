namespace OrdersAPI.DTOs;

// Lo que esperamos que nos responda la API de Usuarios
/// <summary>
/// Modelo interno para mapear la respuesta de la Users.API.
/// </summary>
public record UserExternalResponse
{
    public required Guid Id { get; init; }

    /// <summary>
    /// Indica si el usuario está habilitado (true) o bloqueado (false).
    /// </summary>
    public required bool Activo { get; init; }
}

/// <summary>
/// Modelo interno para mapear la respuesta de la Products.API.
/// </summary>
public record ProductExternalResponse
{
    public required Guid Id { get; init; }

    /// <summary>
    /// Requerido para armar el mensaje de error de ORD-005.
    /// </summary>
    public required string Nombre { get; init; }

    /// <summary>
    /// Requerido para calcular el monto Total de la orden.
    /// </summary>
    public required decimal Precio { get; init; }

    /// <summary>
    /// Requerido para validar si hay stock suficiente (ORD-005).
    /// </summary>
    public required int Stock { get; init; }
}