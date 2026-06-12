using Microsoft.AspNetCore.Mvc;
using ProductsAPI.DTOs;
using ProductsAPI.Services;
using ProductsAPI.Exceptions;

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
    /// <remarks>
    /// Permite obtener el catálogo completo. Soporta filtrado opcional por categoría y nombre.
    /// 
    /// Ejemplo de respuesta exitosa (200 OK):
    /// 
    ///     GET /api/products?categoria=Electrónica
    ///     [
    ///       {
    ///         "id": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
    ///         "nombre": "Notebook Dell XPS 15",
    ///         "descripcion": "Laptop 15 pulgadas, 32GB RAM",
    ///         "precio": 1500.00,
    ///         "stock": 10,
    ///         "categoria": "Electrónica",
    ///         "fechaCreacion": "2024-01-15T10:30:00Z"
    ///       }
    ///     ]
    /// 
    /// Ejemplo de respuesta de error (Error Interno):
    /// 
    ///     {
    ///       "type": "https://tools.ietf.org/html/rfc7231#section-6.6.1",
    ///       "title": "Internal Server Error",
    ///       "status": 500,
    ///       "detail": "Error inesperado en servicio o persistencia.",
    ///       "instance": "/api/products",
    ///       "errorCode": "PRD-005",
    ///       "errorMessage": "Error interno al procesar el producto."
    ///     } 
    /// </remarks>
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
    /// <remarks>
    /// Devuelve los detalles exactos de un producto específico.
    /// 
    /// Busca un producto en el sistema utilizando su identificador único (GUID).
    /// 
    /// Ejemplo de respuesta exitosa (200 OK):
    /// 
    ///     GET /api/products/3fa85f64-5717-4562-b3fc-2c963f66afa6
    ///     {
    ///       "id": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
    ///       "nombre": "Notebook Dell XPS 15",
    ///       "descripcion": "Laptop 15 pulgadas, 32GB RAM",
    ///       "precio": 1500.00,
    ///       "stock": 10,
    ///       "categoria": "Electrónica",
    ///       "fechaCreacion": "2024-01-15T10:30:00Z"
    ///     }
    /// 
    /// Ejemplo de respuesta de error (Producto no encontrado):
    /// 
    ///     {
    ///       "type": "https://tools.ietf.org/html/rfc7231#section-6.5.4",
    ///       "title": "Not Found",
    ///       "status": 404,
    ///       "detail": "El recurso solicitado no fue encontrado.",
    ///       "instance": "/api/products/99",
    ///       "errorCode": "PRD-001",
    ///       "errorMessage": "Producto no encontrado."
    ///     }
    /// </remarks>
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
    /// <remarks>
    /// Registra un nuevo producto en la base de datos. Retorna el objeto creado con su nuevo ID.
    /// 
    /// Ejemplo de solicitud exitosa:
    /// 
    ///     POST /api/products
    ///     {
    ///       "nombre": "Notebook Dell XPS 15",
    ///       "descripcion": "Laptop 15 pulgadas, 32GB RAM",
    ///       "precio": 1500.00,
    ///       "stock": 10,
    ///       "categoria": "Electrónica"
    ///     }
    /// 
    /// Ejemplo de respuesta de error (Datos inválidos):
    /// 
    ///     {
    ///       "type": "https://tools.ietf.org/html/rfc7231#section-6.5.1",
    ///       "title": "Bad Request",
    ///       "status": 400,
    ///       "detail": "No se puede procesar la solicitud.",
    ///       "instance": "/api/products",
    ///       "errorCode": "PRD-002",
    ///       "errorMessage": "Los datos del producto son inválidos."
    ///     }
    /// 
    /// Ejemplo de respuesta de error (Nombre duplicado en categoría):
    /// 
    ///     {
    ///       "type": "https://tools.ietf.org/html/rfc7231#section-6.5.9",
    ///       "title": "Conflict",
    ///       "status": 409,
    ///       "detail": "Ya existe un recurso con esos datos.",
    ///       "instance": "/api/products",
    ///       "errorCode": "PRD-003",
    ///       "errorMessage": "Ya existe un producto con ese nombre en la categoría 'Electrónica'."
    ///     } 
    /// </remarks>
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
        if (!ModelState.IsValid)
        {
            throw new ValidationException("PRD-002", "Los datos del producto son inválidos.");
        }

        var product = await _productService.CreateAsync(request);
        return CreatedAtAction(nameof(GetById), new { id = product.Id }, product);
    }

    /// <summary>Actualizar producto existente.</summary>
    /// <remarks>
    /// Sobrescribe los datos de un producto previamente registrado.
    /// 
    /// Ejemplo de solicitud exitosa:
    /// 
    ///     PUT /api/products/3fa85f64-5717-4562-b3fc-2c963f66afa6
    ///     {
    ///       "nombre": "Notebook Dell XPS 15",
    ///       "descripcion": "Laptop 15 pulgadas, 64GB RAM",
    ///       "precio": 1750.00,
    ///       "stock": 8,
    ///       "categoria": "Electrónica"
    ///     }
    /// 
    /// Ejemplo de respuesta exitosa (200 OK):
    /// 
    ///     {
    ///       "id": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
    ///       "nombre": "Notebook Dell XPS 15",
    ///       "descripcion": "Laptop 15 pulgadas, 64GB RAM",
    ///       "precio": 1750.00,
    ///       "stock": 8,
    ///       "categoria": "Electrónica",
    ///       "fechaCreacion": "2024-01-15T10:30:00Z"
    ///     }
    /// 
    /// Ejemplo de respuesta de error (Datos inválidos - PRD-002):
    /// 
    ///     {
    ///       "type": "https://tools.ietf.org/html/rfc7231#section-6.5.1",
    ///       "title": "Bad Request",
    ///       "status": 400,
    ///       "detail": "No se puede procesar la solicitud.",
    ///       "instance": "/api/products/3fa85f64-5717-4562-b3fc-2c963f66afa6",
    ///       "errorCode": "PRD-002",
    ///       "errorMessage": "Los datos del producto son inválidos."
    ///     }
    ///     
    /// Ejemplo de respuesta de error (Producto no encontrado):
    /// 
    ///     {
    ///       "type": "https://tools.ietf.org/html/rfc7231#section-6.5.4",
    ///       "title": "Not Found",
    ///       "status": 404,
    ///       "detail": "El recurso solicitado no fue encontrado.",
    ///       "instance": "/api/products/3fa85f64-5717-4562-b3fc-2c963f66afa6",
    ///       "errorCode": "PRD-001",
    ///       "errorMessage": "Producto no encontrado."
    ///     } 
    /// </remarks>
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
        if (!ModelState.IsValid)
        {
            throw new ValidationException("PRD-002", "Los datos del producto son inválidos.");
        }

        var product = await _productService.UpdateAsync(id, request);
        return Ok(product);
    }

    /// <summary>Eliminar producto.</summary>
    /// <remarks>
    /// Elimina un producto físicamente. Solo es posible si no tiene órdenes activas asociadas.
    /// 
    /// Ejemplo de solicitud exitosa:
    /// 
    ///     DELETE /api/products/3fa85f64-5717-4562-b3fc-2c963f66afa6
    ///     (Devuelve un 204 No Content sin cuerpo en la respuesta)
    /// 
    /// Ejemplo de respuesta de error (Producto no encontrado - PRD-001):
    /// 
    ///     {
    ///       "type": "https://tools.ietf.org/html/rfc7231#section-6.5.4",
    ///       "title": "Not Found",
    ///       "status": 404,
    ///       "detail": "El recurso solicitado no fue encontrado.",
    ///       "instance": "/api/products/3fa85f64-5717-4562-b3fc-2c963f66afa6",
    ///       "errorCode": "PRD-001",
    ///       "errorMessage": "Producto no encontrado."
    ///     }
    ///     
    /// Ejemplo de respuesta de error (Producto con órdenes activas):
    /// 
    ///     {
    ///       "type": "https://tools.ietf.org/html/rfc7231#section-6.5.9",
    ///       "title": "Conflict",
    ///       "status": 409,
    ///       "detail": "No se puede eliminar el recurso.",
    ///       "instance": "/api/products/3fa85f64-5717-4562-b3fc-2c963f66afa6",
    ///       "errorCode": "PRD-004",
    ///       "errorMessage": "El producto tiene órdenes activas y no puede eliminarse."
    ///     }
    /// </remarks>
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