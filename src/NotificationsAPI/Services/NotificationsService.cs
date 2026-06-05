using NotificationsAPI.DTOs;
using NotificationsAPI.Exceptions;
using NotificationsAPI.Models;
using NotificationsAPI.Data;

namespace NotificationsAPI.Services;

public class NotificationsService : INotificationsService
{
    private readonly NotificationsRepository _repository;
    private readonly ILogger<NotificationsService> _logger;
    private readonly HttpClient _httpClient;

    public NotificationsService(
        NotificationsRepository repository,
        ILogger<NotificationsService> logger,
        IHttpClientFactory httpClientFactory)
    {
        _repository = repository;
        _logger = logger;
        // El cliente "UsersAPI" debe estar configurado en Program.cs
        _httpClient = httpClientFactory.CreateClient("UsersAPI");
    }

    public async Task<NotificationResponse> CreateAsync(CreateNotificationRequest request)
    {
        // LOG DE INFORMACIÓN: Inicio del proceso (útil para auditoría)
        _logger.LogInformation("Iniciando registro y simulación de notificación para el usuario {UsuarioId}", request.UsuarioId);

        // 1. Validar existencia del usuario en la Users.API (Requerimiento 5.5)
        var userResponse = await _httpClient.GetAsync($"/api/users/{request.UsuarioId}");
        if (!userResponse.IsSuccessStatusCode)
        {
            // LOG DE ADVERTENCIA: Error de negocio NTF-001
            _logger.LogWarning("Validación fallida: El usuario {UsuarioId} no existe en Users.API. Error NTF-001.", request.UsuarioId);
            throw new NotFoundException("NTF-001", "El usuario destinatario no fue encontrado.");
        }

        // 2. Crear la entidad de dominio
        var entity = new Notification
        {
            UsuarioId = request.UsuarioId,
            Mensaje = request.Mensaje,
            Tipo = request.Tipo,
            Estado = "Enviada",
            FechaEnvio = DateTime.UtcNow // Asignado al final del registro
        };

        var created = await _repository.CreateAsync(entity);

        // LOG DE INFORMACIÓN: Éxito de la operación
        _logger.LogInformation("Notificación {NotificacionId} enviada y registrada con éxito para el usuario {UsuarioId}", created.Id, created.UsuarioId);

        // 3. Mapeo manual de Entidad a DTO
        return new NotificationResponse(
            created.Id,
            created.UsuarioId,
            created.Mensaje,
            created.Tipo,
            created.Estado,
            created.FechaEnvio);
    }

    public async Task<IEnumerable<NotificationResponse>> GetAllByUsuarioIdAsync(Guid usuarioId)
    {
        // LOG DE INFORMACIÓN: Trazabilidad de consultas
        _logger.LogInformation("Consultando historial de notificaciones para el usuario {UsuarioId}", usuarioId);

        var notifications = await _repository.GetAllByUsuarioIdAsync(usuarioId);

        // Sello de la Cátedra: Si la lista está vacía, lanzar 404
        if (notifications == null || !notifications.Any())
        {
            // LOG DE ADVERTENCIA: Error de negocio NTF-003
            _logger.LogWarning("No se encontraron notificaciones para el usuario {UsuarioId}. Error NTF-003.", usuarioId);
            throw new NotFoundException("NTF-003", "No se encontraron notificaciones para el usuario.");
        }

        // LOG DE INFORMACIÓN: Cantidad de registros recuperados
        _logger.LogInformation("Se recuperaron {Cantidad} notificaciones para el usuario {UsuarioId}", notifications.Count(), usuarioId);

        // Mapeo de colección
        return notifications.Select(n => new NotificationResponse(
            n.Id,
            n.UsuarioId,
            n.Mensaje,
            n.Tipo,
            n.Estado,
            n.FechaEnvio));
    }
}