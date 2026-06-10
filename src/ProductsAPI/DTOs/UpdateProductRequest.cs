using System.ComponentModel.DataAnnotations;

namespace ProductsAPI.DTOs;


/// <summary>
/// Modelo para actualizar un producto existente.
/// </summary>
public record UpdateProductRequest
{
    /// <summary>Nombre del producto.</summary>
    /// <example>Notebook Dell XPS 15</example>
    [Required(ErrorMessage = "El nombre del producto es obligatorio.")]
    [MaxLength(100, ErrorMessage = "El nombre no puede superar los 100 caracteres.")]
    public required string Nombre { get; init; }

    /// <summary>Descripción del producto.</summary>
    /// <example>Laptop 15 pulgadas, 64GB RAM</example>
    [MaxLength(500, ErrorMessage = "La descripción no puede superar los 500 caracteres.")]
    public string? Descripcion { get; init; }

    /// <summary>Precio del producto.</summary>
    /// <example>1750.00</example>
    [Required(ErrorMessage = "El precio es obligatorio.")]
    [Range(0.01, double.MaxValue, ErrorMessage = "El precio debe ser mayor a 0.")]
    public required decimal Precio { get; init; }

    /// <summary>Cantidad de stock disponible.</summary>
    /// <example>8</example>
    [Required(ErrorMessage = "El stock es obligatorio.")]
    [Range(0, int.MaxValue, ErrorMessage = "El stock debe ser mayor o igual a 0.")]
    public required int Stock { get; init; }

    /// <summary>Categoría del producto.</summary>
    /// <example>Electrónica</example>
    [Required(ErrorMessage = "La categoría es obligatoria.")]
    public required string Categoria { get; init; }
}