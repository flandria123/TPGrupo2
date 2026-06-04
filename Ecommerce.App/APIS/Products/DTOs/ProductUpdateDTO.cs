using System.ComponentModel.DataAnnotations;

namespace Ecommerce.App.APIS.Products.DTOs;

/// <summary>
/// DTO utilizado para actualizar un producto existente (Request en PUT)
/// </summary>
public class ProductUpdateDto
{
    /// <summary>
    /// Nuevo nombre del producto
    /// </summary>
    [Required(ErrorMessage = "El nombre es obligatorio")]
    [MaxLength(150)]
    public string Nombre { get; set; } = string.Empty;

    /// <summary>
    /// Nueva descripción
    /// </summary>
    [MaxLength(500)]
    public string? Descripcion { get; set; }

    /// <summary>
    /// Nuevo precio
    /// </summary>
    [Required]
    [Range(0.01, double.MaxValue)]
    public decimal Precio { get; set; }

    /// <summary>
    /// Nuevo stock
    /// </summary>
    [Required]
    [Range(0, int.MaxValue)]
    public int Stock { get; set; }

    /// <summary>
    /// Nueva categoría
    /// </summary>
    [Required]
    public string Categoria { get; set; } = string.Empty;
}