using System.ComponentModel.DataAnnotations;

namespace CartAPI.DTOs
{
    /// <summary>
    /// Objeto con los datos para agregar un producto al carrito.
    /// </summary>
    public record AddItemRequest
    {
        /// <summary>
        /// Identificador único del producto en la Products.API.
        /// </summary>
        /// <example>3fa85f64-5717-4562-b3fc-2c963f66afa6</example>
        [Required(ErrorMessage = "El ID del producto es obligatorio.")]
        public required Guid ProductoId { get; init; }

        /// <summary>
        /// Cantidad de unidades a agregar.
        /// </summary>
        /// <example>2</example>
        [Required(ErrorMessage = "La cantidad es obligatoria.")]
        [Range(1, int.MaxValue, ErrorMessage = "La cantidad debe ser mayor a cero.")]
        public required int Cantidad { get; init; }
    }

}