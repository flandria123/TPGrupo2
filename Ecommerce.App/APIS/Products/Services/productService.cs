using Ecommerce.App.APIS.Products.DTOs;
using Ecommerce.App.Products.API.Exceptions;
using Ecommerce.App.APIS.Products.Models;
using Microsoft.Extensions.Logging;

namespace Ecommerce.App.APIS.Products.Services;

/// <summary>
/// Implementación de la lógica de negocio para la gestión de productos.
/// </summary>
public class ProductService : IProductService
{
    /*private readonly libreria<Product> _libreria;  sumar Librería*/
    private readonly ILogger<ProductService> _logger;

    public ProductService(/*libreria*/<Product> /*libreria*/, ILogger<ProductService> _logger)
    {
        /*_libreria = libreria;*/
        this._logger = _logger;
    }

    public async Task<IEnumerable<Product>> GetAllAsync(string? categoria = null, string? nombre = null)
    {
        _logger.LogInformation("Consultando productos. Filtros -> Categoría: {Categoria}, Nombre: {Nombre}",
            categoria ?? "todos", nombre ?? "todos");

        var products = await /*libreria*/.GetAllAsync();

        if (!string.IsNullOrEmpty(categoria))
            products = products.Where(p => p.Categoria.Equals(categoria, StringComparison.OrdinalIgnoreCase));

        if (!string.IsNullOrEmpty(nombre))
            products = products.Where(p => p.Nombre.Contains(nombre, StringComparison.OrdinalIgnoreCase));

        return products;
    }

    public async Task<Product> GetByIdAsync(Guid id)
    {
        var product = await /*libreria*/.GetByIdAsync(id);

        if (product == null)
            throw new NotFoundException("PRD-001", "Producto no encontrado.");

        return product;
    }

    public async Task<Product> CreateAsync(ProductCreateDto dto)
    {
        _logger.LogInformation("Intentando crear producto: {Nombre} en categoría {Categoria}",
            dto.Nombre, dto.Categoria);

        // Validación de duplicado
        var existing = await /*libreria*/.GetByNameAndCategoryAsync(dto.Nombre, dto.Categoria); // método de la librería
        if (existing != null)
            throw new BusinessRuleException("PRD-003",
                $"Ya existe un producto con ese nombre en la categoría '{dto.Categoria}'.");

        var product = new Product
        {
            Nombre = dto.Nombre,
            Descripcion = dto.Descripcion,
            Precio = dto.Precio,
            Stock = dto.Stock,
            Categoria = dto.Categoria
        };

        await /*libreria*/.AddAsync(product);

        _logger.LogInformation("Producto creado exitosamente. ID: {Id}", product.Id);
        return product;
    }

    public async Task<Product> UpdateAsync(Guid id, ProductUpdateDto dto)
    {
        var existing = await /*libreria*/.GetByIdAsync(id);
        if (existing == null)
            throw new NotFoundException("PRD-001", "Producto no encontrado.");

        // Actualizar campos
        existing.Nombre = dto.Nombre;
        existing.Descripcion = dto.Descripcion;
        existing.Precio = dto.Precio;
        existing.Stock = dto.Stock;
        existing.Categoria = dto.Categoria;

        await /*libreria*/.UpdateAsync(existing);
        _logger.LogInformation("Producto actualizado. ID: {Id}", id);

        return existing;
    }

    public async Task DeleteAsync(Guid id)
    {
        var product = await /*libreria*/.GetByIdAsync(id);
        if (product == null)
            throw new NotFoundException("PRD-001", "Producto no encontrado.");

        // Validación de negocio: si tiene órdenes activas
        bool hasActiveOrders = await /*libreria*/.HasActiveOrdersAsync(id); // método de la librería
        if (hasActiveOrders)
            throw new BusinessRuleException("PRD-004",
                "El producto tiene órdenes activas y no puede eliminarse.");

        await /*libreria*/.DeleteAsync(id);
        _logger.LogInformation("Producto eliminado. ID: {Id}", id);
    }
}