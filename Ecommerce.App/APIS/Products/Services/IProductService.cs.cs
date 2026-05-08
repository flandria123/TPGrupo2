using Ecommerce.App.APIS.Products.DTOs;
using Ecommerce.App.APIS.Products.Models;

namespace Ecommerce.App.APIS.Products.Services;

/// <summary>
/// Interfaz que define los contratos de la lógica de negocio para Productos.
/// </summary>
public interface IProductService
{
    /// <summary>
    /// Obtiene todos los productos, permitiendo filtrar por categoría y/o nombre.
    /// </summary>
    Task<IEnumerable<Product>> GetAllAsync(string? categoria = null, string? nombre = null);

    /// <summary>
    /// Obtiene un producto por su ID.
    /// Lanza NotFoundException si no existe.
    /// </summary>
    Task<Product> GetByIdAsync(Guid id);

    /// <summary>
    /// Crea un nuevo producto.
    /// Valida duplicados por nombre + categoría.
    /// </summary>
    Task<Product> CreateAsync(ProductCreateDto dto);

    /// <summary>
    /// Actualiza un producto existente.
    /// </summary>
    Task<Product> UpdateAsync(Guid id, ProductUpdateDto dto);

    /// <summary>
    /// Elimina un producto.
    /// Valida si tiene órdenes activas antes de eliminar.
    /// </summary>
    Task DeleteAsync(Guid id);
}