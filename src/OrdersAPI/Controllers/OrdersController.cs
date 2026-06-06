using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using OrdersAPI.DTOs;
using OrdersAPI.DTOS;
using OrdersAPI.Models;
using OrdersAPI.Services;

namespace OrdersAPI.Controllers
{
    [ApiController]
    [Route("api/orders")]
    [Tags("Orders")]
    [Produces("application/json")]
    public class OrdersController : ControllerBase
    {
        private readonly IOrderService _orderService;

        public OrdersController(IOrderService orderService)
        {
            _orderService = orderService;
        }

        /// <summary>
        /// Obtiene el listado de órdenes del sistema, con opción de filtrar por usuario.
        /// </summary>
        /// <param name="usuarioId">ID opcional del usuario para filtrar su historial de órdenes.</param>
        /// <response code="200">Listado de órdenes obtenido con éxito.</response>
        /// <response code="400">La solicitud contiene parámetros inválidos (ORD-002).</response>
        /// <response code="500">Error interno al procesar el listado.</response>
        [HttpGet]
        [ProducesResponseType(typeof(IEnumerable<Order>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetOrders([FromQuery] Guid? usuarioId)
        {
            var orders = await _orderService.GetOrdersAsync(usuarioId);
            return Ok(orders);
        }

        /// <summary>
        /// Obtiene el detalle de una orden específica mediante su ID.
        /// </summary>
        /// <remarks>
        /// Ejemplo de respuesta de error (Orden no encontrada):
        /// 
        ///     {
        ///        "type": "https://tools.ietf.org/html/rfc7231#section-6.5.4",
        ///        "title": "Not Found",
        ///        "status": 404,
        ///        "detail": "El recurso solicitado no fue encontrado.",
        ///        "instance": "/api/orders/f47ac10b-58cc-4372-a567-0e02b2c3d479",
        ///        "errorCode": "ORD-001",
        ///        "errorMessage": "La orden con el ID especificado no existe."
        ///     }
        /// 
        /// </remarks>
        /// <param name="id">ID único de la orden (Guid).</param>
        /// <response code="200">Detalle de la orden obtenido con éxito.</response>
        /// <response code="404">La orden solicitada no existe (ORD-001).</response>
        [HttpGet("{id}")]
        [ProducesResponseType(typeof(Order), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetOrderById(Guid id)
        {
            var order = await _orderService.GetOrderByIdAsync(id);
            return Ok(order);
        }

        /// <summary>
        /// Crea una nueva orden de compra en el sistema y calcula su total automáticamente.
        /// </summary>
        /// <remarks>
        /// Ejemplo de solicitud exitosa:
        /// 
        ///     POST /api/orders
        ///     {
        ///        "usuarioId": "a1b2c3d4-0000-0000-0000-111122223333",
        ///        "items": [
        ///           {
        ///              "productoId": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
        ///              "cantidad": 2
        ///           }
        ///        ]
        ///     }
        /// 
        /// Ejemplo de respuesta de error (Stock insuficiente):
        /// 
        ///     {
        ///        "type": "https://tools.ietf.org/html/rfc4918#section-11.2",
        ///        "title": "Unprocessable Entity",
        ///        "status": 422,
        ///        "detail": "No se puede procesar la solicitud.",
        ///        "instance": "/api/orders",
        ///        "errorCode": "ORD-005",
        ///        "errorMessage": "Stock insuficiente para 'Notebook Dell XPS 15'. Disponible: 2, solicitado: 5."
        ///     }
        /// 
        /// </remarks>
        /// <param name="request">Datos requeridos para la creación del pedido (Usuario e Ítems).</param>
        /// <response code="201">Orden creada con éxito.</response>
        /// <response code="400">Los datos de la orden son inválidos (ORD-002).</response>
        /// <response code="404">Usuario no encontrado (ORD-003) o Producto no encontrado (ORD-004).</response>
        /// <response code="422">Stock insuficiente para uno o más productos (ORD-005).</response>
        /// <response code="500">Error interno al procesar la orden (ORD-007).</response>
        [HttpPost]
        [ProducesResponseType(typeof(Order), StatusCodes.Status201Created)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status422UnprocessableEntity)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> CreateOrder([FromBody] CreateOrderRequest request)
        {
            var response = await _orderService.CreateOrderAsync(request);
            return CreatedAtAction(nameof(GetOrderById), new { id = response.Id }, response);
        }

        /// <summary>
        /// Actualiza el estado de una orden.
        /// </summary>
        /// <remarks>
        /// Ejemplo de respuesta de error (Transición inválida):
        /// 
        ///     {
        ///        "type": "https://tools.ietf.org/html/rfc7231#section-6.5.9",
        ///        "title": "Conflict",
        ///        "status": 409,
        ///        "detail": "No se puede modificar el estado.",
        ///        "instance": "/api/orders/f1e2d3c4-0000-0000-0000-aabbccddeeff/status",
        ///        "errorCode": "ORD-006",
        ///        "errorMessage": "No se puede modificar el estado de una orden que ya se encuentra en estado Cancelada o Completada."
        ///     }
        /// 
        /// </remarks>
        /// <param name="id">ID único de la orden (Guid).</param>
        /// <param name="request">Objeto que contiene el nuevo estado de la orden (ej. Confirmada, Cancelada).</param>
        /// <response code="200">Estado de la orden actualizado con éxito. Devuelve la orden actualizada.</response>
        /// <response code="400">El formato de la petición es inválido.</response>
        /// <response code="404">La orden especificada no existe (ORD-001).</response>
        /// <response code="409">Transición de estado inválida según las reglas de negocio (ORD-006).</response>
        [HttpPut("{id}/status")]
        [ProducesResponseType(typeof(UpdateStatusResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
        public async Task<IActionResult> UpdateStatus(Guid id, [FromBody] UpdateStatusRequest request)
        {
            // Pasamos el request.Estado al servicio
            var response = await _orderService.UpdateStatusAsync(id, request.Estado);

            // Devolvemos 200 OK con el nuevo DTO
            return Ok(response);
        }
    }
}