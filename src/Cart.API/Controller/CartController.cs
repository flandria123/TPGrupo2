using Microsoft.AspNetCore.Mvc;
using CartAPI.Services;
using CartAPI.DTOs;
using CartAPI.Exceptions;

namespace CartAPI.Controllers
{
    
    [ApiController]
    [Route("api/cart")]
    [Tags("Cart")] 
    public class CartController : ControllerBase
    {
        private readonly ICartService _cartService;

        public CartController(ICartService cartService)
        {
            _cartService = cartService;
        }

        /// <summary>Obtener carrito del usuario.</summary>
        /// <remarks>
        /// Retorna el carrito activo de un usuario con todos sus ítems.
        /// 
        /// Ejemplo de respuesta exitosa (200 OK):
        /// 
        ///     GET /api/cart/a1b2c3d4-0000-0000-0000-111122223333
        ///     {
        ///       "usuarioId": "a1b2c3d4-0000-0000-0000-111122223333",
        ///       "items": [
        ///         {
        ///           "productoId": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
        ///           "cantidad": 1
        ///         },
        ///         {
        ///           "productoId": "aaaabbbb-cccc-dddd-eeee-ffff00001111",
        ///           "cantidad": 3
        ///         }
        ///       ],
        ///       "fechaActualizacion": "2024-03-10T10:45:00Z"
        ///     }
        /// 
        /// Ejemplo de respuesta de error (Carrito no encontrado):
        /// 
        ///     {
        ///       "type": "https://tools.ietf.org/html/rfc7231#section-6.5.4",
        ///       "title": "Not Found",
        ///       "status": 404,
        ///       "detail": "El recurso solicitado no fue encontrado.",
        ///       "instance": "/api/cart/a1b2c3d4-0000-0000-0000-111122223333",
        ///       "errorCode": "CRT-001",
        ///       "errorMessage": "Carrito no encontrado."
        ///     } 
        /// </remarks>
        /// <param name="userId">ID del usuario dueño del carrito.</param>
        /// <response code="200">Carrito obtenido con éxito.</response>
        /// <response code="404">Carrito no encontrado (CRT-001).</response>
        /// <response code="500">Error interno al procesar el carrito (CRT-005).</response>
        [HttpGet("{userId}")]
        [ProducesResponseType(typeof(CartResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<CartResponse>> GetCart(Guid userId)
        {
            var cart = await _cartService.GetCartAsync(userId);
            return Ok(cart);
        }

        /// <summary>Agregar producto al carrito.</summary>
        /// <remarks>
        /// Agrega un producto nuevo o suma la cantidad si ya existe. Valida stock contra Products.API.
        /// 
        /// Ejemplo de solicitud exitosa:
        /// 
        ///     POST /api/cart/a1b2c3d4-0000-0000-0000-111122223333/items
        ///     {
        ///       "productoId": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
        ///       "cantidad": 2
        ///     }
        /// 
        /// Ejemplo de respuesta exitosa (200 OK):
        /// 
        ///     {
        ///       "usuarioId": "a1b2c3d4-0000-0000-0000-111122223333",
        ///       "items": [
        ///         {
        ///           "productoId": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
        ///           "cantidad": 2
        ///         }
        ///       ],
        ///       "fechaActualizacion": "2024-03-10T10:50:00Z"
        ///     }
        /// 
        /// Ejemplo de respuesta de error (Stock insuficiente):
        /// 
        ///     {
        ///       "type": "https://tools.ietf.org/html/rfc4918#section-11.2",
        ///       "title": "Unprocessable Entity",
        ///       "status": 422,
        ///       "detail": "No se puede procesar la solicitud.",
        ///       "instance": "/api/cart/a1b2c3d4/items",
        ///       "errorCode": "CRT-003",
        ///       "errorMessage": "Stock insuficiente. Disponible: 1, solicitado: 5."
        ///     } 
        /// </remarks>
        /// <param name="userId">ID del usuario.</param>
        /// <param name="request">Datos del ítem a agregar (ProductoId y Cantidad).</param>
        /// <response code="200">Producto agregado con éxito.</response>
        /// <response code="400">Cantidad inválida (CRT-004).</response>
        /// <response code="404">Producto no encontrado en Products.API (CRT-002).</response>
        /// <response code="422">Stock insuficiente (CRT-003).</response>
        /// <response code="500">Error interno (CRT-005).</response>
        [HttpPost("{userId}/items")]
        [ProducesResponseType(typeof(CartResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status422UnprocessableEntity)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<CartResponse>> AddItem(Guid userId, [FromBody] AddItemRequest request)
        {
            
            if (!ModelState.IsValid)
            {
                var errores = string.Join("; ", ModelState.Values
                    .SelectMany(v => v.Errors)
                    .Select(e => e.ErrorMessage));

                throw new ValidationException("CRT-004", errores); 
            }

           
            var cart = await _cartService.AddItemAsync(userId, request);
            return Ok(cart);
        }

        /// <summary>Actualizar cantidad de un ítem.</summary>
        /// <remarks>
        /// Reemplaza la cantidad actual de un producto específico en el carrito.
        /// 
        /// Ejemplo de solicitud exitosa:
        /// 
        ///     PUT /api/cart/a1b2c3d4-0000-0000-0000-111122223333/items/3fa85f64-5717-4562-b3fc-2c963f66afa6
        ///     {
        ///       "cantidad": 4
        ///     }
        /// 
        /// Ejemplo de respuesta exitosa (200 OK):
        /// 
        ///     {
        ///       "usuarioId": "a1b2c3d4-0000-0000-0000-111122223333",
        ///       "items": [
        ///         {
        ///           "productoId": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
        ///           "cantidad": 4
        ///         }
        ///       ],
        ///       "fechaActualizacion": "2024-03-10T11:05:00Z"
        ///     }
        /// 
        /// Ejemplo de respuesta de error (Producto no encontrado):
        /// 
        ///     {
        ///       "type": "https://tools.ietf.org/html/rfc7231#section-6.5.4",
        ///       "title": "Not Found",
        ///       "status": 404,
        ///       "detail": "El recurso solicitado no fue encontrado.",
        ///       "instance": "/api/cart/a1b2c3d4-0000-0000-0000-111122223333/items/3fa85f64-5717-4562-b3fc-2c963f66afa6",
        ///       "errorCode": "CRT-002",
        ///       "errorMessage": "Producto no encontrado."
        ///     } 
        /// </remarks>
        /// <param name="userId">ID del usuario.</param>
        /// <param name="productId">ID del producto a actualizar.</param>
        /// <param name="request">Nueva cantidad.</param>
        /// <response code="200">Cantidad actualizada con éxito.</response>
        /// <response code="400">Cantidad inválida (CRT-004).</response>
        /// <response code="404">Carrito o producto no encontrado (CRT-001, CRT-002).</response>
        /// <response code="422">Stock insuficiente (CRT-003).</response>
        /// <response code="500">Error interno (CRT-005).</response>
        [HttpPut("{userId}/items/{productId}")]
        [ProducesResponseType(typeof(CartResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status422UnprocessableEntity)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<CartResponse>> UpdateItem(Guid userId, Guid productId, [FromBody] UpdateItemRequest request)
        {
            // Sello de la Cátedra (Nivel Experto): Extracción dinámica de errores [cite: 4]
            if (!ModelState.IsValid)
            {
                var errores = string.Join("; ", ModelState.Values
                    .SelectMany(v => v.Errors)
                    .Select(e => e.ErrorMessage));

                throw new ValidationException("CRT-004", errores); // Disparamos el error 400 [cite: 25]
            }

            var cart = await _cartService.UpdateItemAsync(userId, productId, request);
            return Ok(cart);
        }

        /// <summary>Quitar un producto del carrito.</summary>
        /// <remarks>
        /// Elimina el ítem completamente. Si el carrito queda vacío, se elimina la cabecera.
        /// 
        /// Ejemplo de solicitud exitosa:
        /// 
        ///     DELETE /api/cart/a1b2c3d4-0000-0000-0000-111122223333/items/3fa85f64-5717-4562-b3fc-2c963f66afa6
        ///     (Devuelve un 204 No Content sin cuerpo en la respuesta)
        /// 
        /// Ejemplo de respuesta de error (Producto no encontrado en el carrito):
        /// 
        ///     {
        ///       "type": "https://tools.ietf.org/html/rfc7231#section-6.5.4",
        ///       "title": "Not Found",
        ///       "status": 404,
        ///       "detail": "El recurso solicitado no fue encontrado.",
        ///       "instance": "/api/cart/a1b2c3d4-0000-0000-0000-111122223333/items/3fa85f64-5717-4562-b3fc-2c963f66afa6",
        ///       "errorCode": "CRT-002",
        ///       "errorMessage": "Producto no encontrado."
        ///     }
        /// </remarks>
        /// <param name="userId">ID del usuario.</param>
        /// <param name="productId">ID del producto a eliminar.</param>
        /// <response code="204">Producto eliminado con éxito.</response>
        /// <response code="404">Carrito o producto no encontrado (CRT-001, CRT-002).</response>
        /// <response code="500">Error interno (CRT-005).</response>
        [HttpDelete("{userId}/items/{productId}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> RemoveItem(Guid userId, Guid productId)
        {
            await _cartService.RemoveItemAsync(userId, productId);
            return NoContent();
        }

        /// <summary>Vaciar carrito completo.</summary>
        /// <remarks>
        /// Elimina de forma permanente el carrito y todos sus ítems.
        /// 
        /// Ejemplo de solicitud exitosa:
        /// 
        ///     DELETE /api/cart/a1b2c3d4-0000-0000-0000-111122223333
        ///     (Devuelve un 204 No Content sin cuerpo en la respuesta)
        /// 
        /// Ejemplo de respuesta de error (Carrito no encontrado):
        /// 
        ///     {
        ///       "type": "https://tools.ietf.org/html/rfc7231#section-6.5.4",
        ///       "title": "Not Found",
        ///       "status": 404,
        ///       "detail": "El recurso solicitado no fue encontrado.",
        ///       "instance": "/api/cart/a1b2c3d4-0000-0000-0000-111122223333",
        ///       "errorCode": "CRT-001",
        ///       "errorMessage": "Carrito no encontrado."
        ///     } 
        /// </remarks>
        /// <param name="userId">ID del usuario dueño del carrito.</param>
        /// <response code="204">Carrito vaciado con éxito.</response>
        /// <response code="404">Carrito no encontrado (CRT-001).</response>
        /// <response code="500">Error interno (CRT-005).</response>
        [HttpDelete("{userId}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> ClearCart(Guid userId)
        {
            await _cartService.ClearCartAsync(userId);
            return NoContent();
        }
    }
}