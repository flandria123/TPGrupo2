using System.ComponentModel.DataAnnotations;

namespace CartAPI.DTOs
{

    /// <summary>
    /// Objeto con los datos para actualizar la cantidad de un producto existente en el carrito.
    /// </summary>
    public record UpdateItemRequest
    {
        /// <summary>
        /// Nueva cantidad de unidades del producto.
        /// </summary>
        /// <example>4</example>
        [Required(ErrorMessage = "La cantidad es obligatoria.")]
        [Range(1, int.MaxValue, ErrorMessage = "La cantidad debe ser mayor a cero.")]
        public required int Cantidad { get; init; }
    }
}