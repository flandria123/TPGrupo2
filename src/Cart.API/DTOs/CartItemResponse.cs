namespace CartAPI.DTOs
{
    /// <summary>
    /// Objeto que representa un producto y su cantidad dentro del carrito.
    /// </summary>
    public record CartItemResponse(

        /// <summary>
        /// Identificador único del producto.
        /// </summary>
        /// <example>3fa85f64-5717-4562-b3fc-2c963f66afa6</example>
        Guid ProductoId,

        /// <summary>
        /// Cantidad de unidades del producto en el carrito.
        /// </summary>
        /// <example>2</example>
        int Cantidad
    );




}