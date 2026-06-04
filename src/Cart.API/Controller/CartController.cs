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
        /// <remarks>Retorna el carrito activo de un usuario con todos sus ítems.</remarks>
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
        /// <remarks>Agrega un producto nuevo o suma la cantidad si ya existe. Valida stock contra Products.API.</remarks>
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
        /// <remarks>Reemplaza la cantidad actual de un producto específico en el carrito.</remarks>
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
        /// <remarks>Elimina el ítem completamente. Si el carrito queda vacío, se elimina la cabecera.</remarks>
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
        /// <remarks>Elimina de forma permanente el carrito y todos sus ítems.</remarks>
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