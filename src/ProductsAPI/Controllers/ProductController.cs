using Microsoft.AspNetCore.Mvc;
using ProductsAPI.DTOs;
using ProductsAPI.Services;

namespace ProductsAPI.Controllers;

[ApiController]
[Route("api/products")]
[Produces("application/json")]
public class ProductsController : ControllerBase
{
    private readonly IProductService _productService;

    public ProductsController(IProductService productService)
    {
        _productService = productService;
    }

    /// <summary>Listar productos.</summary>
    /// <remarks>Permite obtener el catálogo completo. Soporta filtrado opcional por categoría y nombre.</remarks>
    /// <param name="categoria">Filtro opcional por categoría (ej. Electrónica).</param>
    /// <param name="nombre">Filtro opcional por coincidencia de nombre.</param>
    /// <response code="200">Lista de productos devuelta con éxito.</response>
    /// <response code="500">Error interno al procesar la solicitud (PRD-005).</response>
    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<ProductResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<IEnumerable<ProductResponse>>> GetAll(
        [FromQuery] string? categoria,
        [FromQuery] string? nombre)
    {
        var products = await _productService.GetAllAsync(categoria, nombre);
        return Ok(products);
    }

    /// <summary>Obtener producto por ID.</summary>
    /// <remarks>Devuelve los detalles exactos de un producto específico.</remarks>
    /// <param name="id">El identificador único (Guid) del producto.</param>
    /// <response code="200">Producto encontrado y devuelto con éxito.</response>
    /// <response code="404">Producto no encontrado (PRD-001).</response>
    /// <response code="500">Error interno al procesar el producto (PRD-005).</response>
    [HttpGet("{id}")]
    [ProducesResponseType(typeof(ProductResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<ProductResponse>> GetById(Guid id)
    {
        var product = await _productService.GetByIdAsync(id);
        return Ok(product);
    }

    /// <summary>Crear nuevo producto.</summary>
    /// <remarks>Registra un nuevo producto en la base de datos. Retorna el objeto creado con su nuevo ID.</remarks>
    /// <param name="request">Los datos del producto a crear.</param>
    /// <response code="201">Producto creado con éxito.</response>
    /// <response code="400">Los datos del producto son inválidos (PRD-002).</response>
    /// <response code="409">Ya existe un producto con ese nombre en la categoría (PRD-003).</response>
    /// <response code="500">Error interno al procesar el producto (PRD-005).</response>
    [HttpPost]
    [ProducesResponseType(typeof(ProductResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<ProductResponse>> Create([FromBody] CreateProductRequest request)
    {
        var product = await _productService.CreateAsync(request);
        return CreatedAtAction(nameof(GetById), new { id = product.Id }, product);
    }

    /// <summary>Actualizar producto existente.</summary>
    /// <remarks>Sobrescribe los datos de un producto previamente registrado.</remarks>
    /// <param name="id">El ID del producto a modificar.</param>
    /// <param name="request">Los nuevos datos del producto.</param>
    /// <response code="200">Producto actualizado con éxito.</response>
    /// <response code="400">Los datos del producto son inválidos (PRD-002).</response>
    /// <response code="404">Producto no encontrado (PRD-001).</response>
    /// <response code="500">Error interno al procesar el producto (PRD-005).</response>
    [HttpPut("{id}")]
    [ProducesResponseType(typeof(ProductResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<ProductResponse>> Update(Guid id, [FromBody] UpdateProductRequest request)
    {
        var product = await _productService.UpdateAsync(id, request);
        return Ok(product);
    }

    /// <summary>Eliminar producto.</summary>
    /// <remarks>Elimina un producto físicamente. Solo es posible si no tiene órdenes activas asociadas.</remarks>
    /// <param name="id">El ID del producto a eliminar.</param>
    /// <response code="204">Producto eliminado con éxito (Sin contenido).</response>
    /// <response code="404">Producto no encontrado (PRD-001).</response>
    /// <response code="409">El producto tiene órdenes activas y no puede eliminarse (PRD-004).</response>
    /// <response code="500">Error interno al procesar el producto (PRD-005).</response>
    [HttpDelete("{id}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> Delete(Guid id)
    {
        await _productService.DeleteAsync(id);
        return NoContent();
    }
}