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

        // 1. VALIDACIÓN DE NEGOCIO: Campos obligatorios y tipo reconocido (NTF-002)
        if (string.IsNullOrWhiteSpace(request.Tipo) ||
           (request.Tipo != "Email" && request.Tipo != "Push" && request.Tipo != "SMS"))
        {
            _logger.LogWarning("Validación fallida: El tipo de notificación '{Tipo}' es inválido o está vacío. Error NTF-002.", request.Tipo);
            throw new ValidationException("NTF-002", "Los datos de la notificación son inválidos. El tipo no puede estar vacío y debe ser 'Email', 'Push' o 'SMS'.");
        }

        if (string.IsNullOrWhiteSpace(request.Mensaje) || request.Mensaje.Length > 500)
        {
            _logger.LogWarning("Validación fallida: El mensaje está vacío o excede los 500 caracteres. Error NTF-002.");
            throw new ValidationException("NTF-002", "Los datos de la notificación son inválidos. El mensaje es obligatorio y no puede superar los 500 caracteres.");
        }

        // 2. Validar existencia del usuario en la Users.API (Requerimiento 5.5)
        var userResponse = await _httpClient.GetAsync($"/api/users/{request.UsuarioId}");
        if (!userResponse.IsSuccessStatusCode)
        {
            // LOG DE ADVERTENCIA: Error de negocio NTF-001
            _logger.LogWarning("Validación fallida: El usuario {UsuarioId} no existe en Users.API. Error NTF-001.", request.UsuarioId);
            throw new NotFoundException("NTF-001", "El usuario destinatario no fue encontrado.");
        }

        
        // 3. Crear la entidad de dominio
        var entity = new Notification
        {
            Id = Guid.NewGuid(), // <-- Generamos el GUID único de la notificación aquí
            UsuarioId = request.UsuarioId,
            Mensaje = request.Mensaje,
            Tipo = request.Tipo,
            Estado = "Enviada",
            FechaEnvio = DateTime.UtcNow // Asignado al final del registro
        };

        var created = await _repository.CreateAsync(entity);

        // LOG DE INFORMACIÓN: Éxito de la operación
        _logger.LogInformation("Notificación {NotificacionId} enviada y registrada con éxito para el usuario {UsuarioId}", created.Id, created.UsuarioId);

        // 4. Mapeo manual de Entidad a DTO
        return new NotificationResponse
        {
            Id = created.Id,
            UsuarioId = created.UsuarioId,
            Mensaje = created.Mensaje,
            Tipo = created.Tipo,
            Estado = created.Estado,
            FechaEnvio = created.FechaEnvio
        };
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
        return notifications.Select(n => new NotificationResponse
        {
            Id = n.Id,
            UsuarioId = n.UsuarioId,
            Mensaje = n.Mensaje,
            Tipo = n.Tipo,
            Estado = n.Estado,
            FechaEnvio = n.FechaEnvio
        }).ToList(); 
    }
}