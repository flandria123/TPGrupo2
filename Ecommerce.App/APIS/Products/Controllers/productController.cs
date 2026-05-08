using Microsoft.AspNetCore.Mvc;
using Ecommerce.App.APIS.Products.DTOs;
using Ecommerce.App.APIS.Products.Services;
using Ecommerce.App.APIS.Products.Models;

namespace Ecommerce.App.APIS.Products.Controllers;

/// <summary>
/// Controlador principal de la API Product
/// Gestiona todas las operaciones CRUD de product.
/// </summary>
[ApiController]
[Route("api/products")]
[Produces("application/json")]
public class ProductsController : ControllerBase
{
    private readonly IProductService _productService;

    /// <summary>
    /// Constructor con inyección de dependencia del Service
    /// </summary>
    public ProductsController(IProductService productService)
    {
        _productService = productService;
    }

    /// <summary>
    /// Obtiene la lista de todos los productos con filtros opcionales
    /// </summary>
    /// <param name="categoria">Filtrar por categoría</param>
    /// <param name="nombre">Filtrar por nombre parcial</param>
    /// <returns>Lista de productos</returns>
    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<Product>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<IEnumerable<Product>>> GetAll(
        [FromQuery] string? categoria = null,
        [FromQuery] string? nombre = null)
    {
        var products = await _productService.GetAllAsync(categoria, nombre);
        return Ok(products);
    }

    /// <summary>
    /// Obtiene un producto por su ID
    /// </summary>
    /// <param name="IdProduct">GUID del producto</param>
    /// <returns>Producto encontrado</returns>
    [HttpGet("{IdProduct}")]
    [ProducesResponseType(typeof(Product), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<Product>> GetById(Guid IdProduct)
    {
        var product = await _productService.GetByIdAsync(IdProduct);
        return Ok(product);
    }

    /// <summary>
    /// Crea un nuevo producto
    /// </summary>
    /// <param name="dto">Datos del producto a crear</param>
    /// <returns>Producto creado con su ID</returns>
    [HttpPost]
    [ProducesResponseType(typeof(Product), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<Product>> Create([FromBody] ProductCreateDto dto)
    {
        var product = await _productService.CreateAsync(dto);

        return CreatedAtAction(
            nameof(GetById),
            new { IdProduct = product.IdProduct },
            product);
    }

    /// <summary>
    /// Actualiza un producto existente
    /// </summary>
    /// <param name="IdProduct">ID del producto a actualizar</param>
    /// <param name="dto">Datos actualizados</param>
    [HttpPut("{IdProduct}")]
    [ProducesResponseType(typeof(Product), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<Product>> Update(Guid IdProduct, [FromBody] ProductUpdateDto dto)
    {
        var product = await _productService.UpdateAsync(IdProduct, dto);
        return Ok(product);
    }

    /// <summary>
    /// Elimina un producto (con validación de órdenes activas)
    /// </summary>
    /// <param name="IdProduct">ID del producto a eliminar</param>
    [HttpDelete("{IdProduct}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> Delete(Guid IdProduct)
    {
        await _productService.DeleteAsync(IdProduct);
        return NoContent();
    }
}