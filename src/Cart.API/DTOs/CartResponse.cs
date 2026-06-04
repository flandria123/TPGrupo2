namespace CartAPI.DTOs
{

    /// <summary>
    /// Objeto que representa el carrito de compras de un usuario y su contenido.
    /// </summary>
    public record CartResponse(

        /// <summary>
        /// Identificador único del usuario dueño del carrito.
        /// </summary>
        /// <example>a1b2c3d4-0000-0000-0000-111122223333</example>
        Guid UsuarioId,

        /// <summary>
        /// Lista de productos contenidos en el carrito.
        /// </summary>
        IEnumerable<CartItemResponse> Items,

        /// <summary>
        /// Fecha y hora de la última modificación del carrito.
        /// </summary>
        /// <example>2024-03-10T10:45:00Z</example>
        DateTime FechaActualizacion
    );
}