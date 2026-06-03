using Microsoft.AspNetCore.Mvc;
using NotificationsAPI.DTOs;
using NotificationsAPI.Services;

namespace NotificationsAPI.Controllers;

[ApiController]
[Route("api/notifications")] // Ruta base según contrato [3]
[Tags("Notifications")]      // Agrupación obligatoria para Swagger [1]
public class NotificationsController : ControllerBase
{
    private readonly INotificationsService _service;

    public NotificationsController(INotificationsService service)
    {
        _service = service;
    }

    // ─────────────────────────────────────────────
    // POST: /api/notifications/send
    // ─────────────────────────────────────────────

    /// <summary>
    /// Registra y simula el envío de una notificación.
    /// </summary>
    /// <remarks>
    /// Valida la existencia del usuario y registra la notificación en estado 'Enviada'.
    /// </remarks>
    /// <response code="201">Notificación creada y enviada correctamente.</response>
    /// <response code="400">Datos inválidos (ErrorCode: NTF-002).</response>
    /// <response code="404">Usuario destinatario no encontrado (ErrorCode: NTF-001).</response>
    [HttpPost("send")] // Endpoint exacto del contrato [3, 4]
    [ProducesResponseType(typeof(NotificationResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<NotificationResponse>> SendNotification(
        [FromBody] CreateNotificationRequest request)
    {
        // El Sello de la Cátedra: No hay try-catch aquí, se encarga el ExceptionHandler [5, 6]
        var created = await _service.CreateAsync(request);

        // Devolvemos 201 Created con el objeto según el ejemplo del PDF [4, 7]
        return StatusCode(StatusCodes.Status201Created, created);
    }

    // ─────────────────────────────────────────────
    // GET: /api/notifications/{userId}
    // ─────────────────────────────────────────────

    /// <summary>
    /// Obtiene el historial de notificaciones de un usuario.
    /// </summary>
    /// <param name="userId">ID del usuario (GUID).</param>
    /// <response code="200">Lista de notificaciones obtenida correctamente.</response>
    /// <response code="404">No se encontraron notificaciones para el usuario (ErrorCode: NTF-003).</response>
    [HttpGet("{userId:guid}")] // Ruta exacta del contrato [1, 3]
    [ProducesResponseType(typeof(IEnumerable<NotificationResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<IEnumerable<NotificationResponse>>> GetNotificationsByUserId(
        Guid userId)
    {
        var notifications = await _service.GetAllByUsuarioIdAsync(userId);

        return Ok(notifications);
    }
}