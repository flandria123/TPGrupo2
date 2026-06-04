using System.ComponentModel.DataAnnotations;

namespace Ecommerce.App.APIS.Products.DTOs;

/// <summary>
/// DTO utilizado para crear un nuevo producto (Request en POST)
/// No incluye Id ni FechaCreacion porque se generan automáticamente.
/// </summary>
public class ProductCreateDto
{
    /// <summary>
    /// Nombre del producto
    /// </summary>
    [Required(ErrorMessage = "El nombre es obligatorio")]
    [MaxLength(150)]
    public string Nombre { get; set; } = string.Empty;

    /// <summary>
    /// Descripción detallada del producto
    /// </summary>
    [MaxLength(500)]
    public string? Descripcion { get; set; }

    /// <summary>
    /// Precio del producto
    /// </summary>
    [Required(ErrorMessage = "El precio es obligatorio")]
    [Range(0.01, double.MaxValue, ErrorMessage = "El precio debe ser mayor a 0")]
    public decimal Precio { get; set; }

    /// <summary>
    /// Cantidad inicial en stock
    /// </summary>
    [Required(ErrorMessage = "El stock es obligatorio")]
    [Range(0, int.MaxValue, ErrorMessage = "El stock no puede ser negativo")]
    public int Stock { get; set; }

    /// <summary>
    /// Categoría del producto
    /// </summary>
    [Required(ErrorMessage = "La categoría es obligatoria")]
    public string Categoria { get; set; } = string.Empty;
}