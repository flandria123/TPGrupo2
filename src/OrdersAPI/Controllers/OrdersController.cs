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
        /// <remarks>
        /// Ejemplo de respuesta exitosa (200 OK) devolviendo una lista:
        /// 
        ///     GET /api/orders?usuarioId=a1b2c3d4-0000-0000-0000-111122223333
        ///     [
        ///       {
        ///         "id": "f1e2d3c4-0000-0000-0000-aabbccddeeff",
        ///         "usuarioId": "a1b2c3d4-0000-0000-0000-111122223333",
        ///         "items": [
        ///           {
        ///             "productoId": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
        ///             "cantidad": 2,
        ///             "precioUnitario": 1500.00
        ///           }
        ///         ],
        ///         "total": 3000.00,
        ///         "estado": "Pendiente",
        ///         "fechaCreacion": "2024-03-10T11:00:00Z"
        ///       }
        ///     ]
        /// 
        /// </remarks>
        /// <param name="usuarioId">ID opcional del usuario para filtrar su historial de órdenes.</param>
        /// <response code="200">Listado de órdenes obtenido con éxito.</response>
        /// <response code="500">Error interno al procesar el listado (ErrorCode: ORD-007).</response>
        [HttpGet]
        [ProducesResponseType(typeof(IEnumerable<OrderResponse>), StatusCodes.Status200OK)] // Usamos el DTO OrderResponse
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)] // Eliminamos el 400
        public async Task<ActionResult<IEnumerable<OrderResponse>>> GetOrders([FromQuery] Guid? usuarioId)
        {
            var orders = await _orderService.GetOrdersAsync(usuarioId);

            // Asegúrate de que tu servicio internamente mapee las entidades 'Order' a 'OrderResponse' antes de devolverlas
            return Ok(orders);
        }

        /// <summary>
        /// Obtiene el detalle de una orden específica mediante su ID.
        /// </summary>
        /// <remarks>
        /// 
        /// Ejemplo de respuesta exitosa (200 OK):
        /// 
        ///     GET /api/orders/f1e2d3c4-0000-0000-0000-aabbccddeeff
        ///     {
        ///       "id": "f1e2d3c4-0000-0000-0000-aabbccddeeff",
        ///       "usuarioId": "a1b2c3d4-0000-0000-0000-111122223333",
        ///       "items": [
        ///         {
        ///           "productoId": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
        ///           "cantidad": 2,
        ///           "precioUnitario": 1500.00
        ///         }
        ///       ],
        ///       "total": 3000.00,
        ///       "estado": "Pendiente",
        ///       "fechaCreacion": "2024-03-10T11:00:00Z"
        ///     }
        /// 
        /// Ejemplo de respuesta de error (Orden no encontrada):
        /// 
        ///     {
        ///       "type": "https://tools.ietf.org/html/rfc7231#section-6.5.4",
        ///       "title": "Not Found",
        ///       "status": 404,
        ///       "detail": "El recurso solicitado no fue encontrado.",
        ///       "instance": "/api/orders/f1e2d3c4-0000-0000-0000-aabbccddeeff",
        ///       "errorCode": "ORD-001",
        ///       "errorMessage": "Orden no encontrada."
        ///     }
        /// </remarks>
        /// <param name="id">ID único de la orden (Guid).</param>
        /// <response code="200">Detalle de la orden obtenido con éxito.</response>
        /// <response code="404">La orden solicitada no existe (ErrorCode: ORD-001).</response>
        /// <response code="500">Error interno al procesar la solicitud (ErrorCode: ORD-007).</response>
        [HttpGet("{id}")]
        [ProducesResponseType(typeof(OrderResponse), StatusCodes.Status200OK)] // CORREGIDO a OrderResponse
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)] // AGREGADO el 500
        public async Task<ActionResult<OrderResponse>> GetOrderById(Guid id)
        {
            var order = await _orderService.GetOrderByIdAsync(id);

            // Asegúrate de que internamente tu _orderService.GetOrderByIdAsync devuelva un OrderResponse
            // y no la Entidad de la base de datos.
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
        /// Ejemplo de respuesta exitosa (201 Created):
        /// 
        ///     {
        ///       "id": "f1e2d3c4-0000-0000-0000-aabbccddeeff",
        ///       "usuarioId": "a1b2c3d4-0000-0000-0000-111122223333",
        ///       "items": [
        ///         {
        ///           "productoId": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
        ///           "cantidad": 2,
        ///           "precioUnitario": 1500.00
        ///         }
        ///       ],
        ///       "total": 3000.00,
        ///       "estado": "Pendiente",
        ///       "fechaCreacion": "2024-03-10T11:00:00Z"
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
        /// </remarks>
        /// <param name="request">Datos requeridos para la creación del pedido (Usuario e Ítems).</param>
        /// <response code="201">Orden creada con éxito.</response>
        /// <response code="400">Los datos de la orden son inválidos (ErrorCode: ORD-002).</response>
        /// <response code="404">Usuario no encontrado (ErrorCode: ORD-003) o Producto no encontrado (ErrorCode: ORD-004).</response>
        /// <response code="422">Stock insuficiente para uno o más productos (ErrorCode: ORD-005).</response>
        /// <response code="500">Error interno al procesar la orden (ErrorCode: ORD-007).</response>
        [HttpPost]
        [ProducesResponseType(typeof(OrderResponse), StatusCodes.Status201Created)] 
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status422UnprocessableEntity)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<OrderResponse>> CreateOrder([FromBody] CreateOrderRequest request)
        {
            var response = await _orderService.CreateOrderAsync(request);

            // Verifica que el string dentro del nameof() sea exactamente el nombre de tu método GET.
            // Si arriba lo llamaste GetById, pon nameof(GetById)
            return CreatedAtAction(nameof(GetOrderById), new { id = response.Id }, response);
        }

        /// <summary>
        /// Actualiza el estado de una orden.
        /// </summary>
        /// <remarks>
        /// Ejemplo de solicitud exitosa:
        /// 
        ///     PUT /api/orders/f1e2d3c4-0000-0000-0000-aabbccddeeff/status
        ///     {
        ///        "estado": "Confirmada"
        ///     }
        /// 
        /// Ejemplo de respuesta exitosa (200 OK):
        /// 
        ///     {
        ///        "id": "f1e2d3c4-0000-0000-0000-aabbccddeeff",
        ///        "estado": "Confirmada",
        ///        "fechaActualizacion": "2024-03-10T12:00:00Z"
        ///     }
        ///
        /// Ejemplo de respuesta de error (Transición inválida):
        /// 
        ///     {
        ///        "type": "https://tools.ietf.org/html/rfc7231#section-6.5.9",
        ///        "title": "Conflict",
        ///        "status": 409,
        ///        "detail": "No se puede modificar el estado.",
        ///        "instance": "/api/orders/f1e2d3c4-0000-0000-0000-aabbccddeeff/status",
        ///        "errorCode": "ORD-006",
        ///        "errorMessage": "Una orden en estado 'Entregada' no puede volver a 'Pendiente'."
        ///     }
        /// </remarks>
        /// <param name="id">ID único de la orden (Guid).</param>
        /// <param name="request">Objeto que contiene el nuevo estado de la orden (Pendiente, Confirmada, Enviada, Entregada, Cancelada).</param>
        /// <response code="200">Estado de la orden actualizado con éxito. Retorna ID, Estado y FechaActualizacion.</response>
        /// <response code="400">El formato de la petición es inválido (ErrorCode: ORD-002).</response>
        /// <response code="404">La orden especificada no existe (ErrorCode: ORD-001).</response>
        /// <response code="409">Transición de estado inválida según las reglas de negocio (ErrorCode: ORD-006).</response>
        /// <response code="500">Error interno al procesar la orden (ErrorCode: ORD-007).</response>
        [HttpPut("{id}/status")]
        [ProducesResponseType(typeof(UpdateStatusResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<UpdateStatusResponse>> UpdateStatus(Guid id, [FromBody] UpdateStatusRequest request)
        {
            // Pasamos el request.Estado al servicio
            var response = await _orderService.UpdateStatusAsync(id, request.Estado);

            // Devolvemos 200 OK con el nuevo DTO
            return Ok(response);
        }
    }
}