using System.ComponentModel.DataAnnotations;

namespace ProductsAPI.DTOs;


public record UpdateProductRequest(

    /// <example>Notebook Dell XPS 15</example>
    [Required(ErrorMessage = "El nombre del producto es obligatorio.")]
    [MaxLength(100, ErrorMessage = "El nombre no puede superar los 100 caracteres.")]
    string Nombre,

    /// <example>Laptop 15 pulgadas, 64GB RAM</example>
    [MaxLength(500, ErrorMessage = "La descripción no puede superar los 500 caracteres.")]
    string? Descripcion,

    /// <example>1750.00</example>
    [Required(ErrorMessage = "El precio es obligatorio.")]
    [Range(0.01, double.MaxValue, ErrorMessage = "El precio debe ser mayor a 0.")]
    decimal Precio,

    /// <example>8</example>
    [Required(ErrorMessage = "El stock es obligatorio.")]
    [Range(0, int.MaxValue, ErrorMessage = "El stock debe ser mayor o igual a 0.")]
    int Stock,

    /// <example>Electrónica</example>
    [Required(ErrorMessage = "La categoría es obligatoria.")]
    string Categoria

);