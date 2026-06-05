using NotificationsAPI.DTOs;

namespace NotificationsAPI.Services;

public interface INotificationsService
{
    
    Task<NotificationResponse> CreateAsync(CreateNotificationRequest request);

   
    Task<IEnumerable<NotificationResponse>> GetAllByUsuarioIdAsync(Guid userId);
}