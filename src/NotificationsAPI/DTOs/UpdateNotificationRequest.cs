using System.ComponentModel.DataAnnotations;

namespace NotificationsAPI.DTOs
{
    /// <summary>
    /// Payload requerido para actualizar una notificación existente.
    /// </summary>
    public record UpdateNotificationRequest(

        /// <example>Enviado</example>
        [Required(ErrorMessage = "El estado es obligatorio.")]
        string Estado,

        /// <example>Tu orden ha sido procesada con éxito.</example>
        [Required(ErrorMessage = "El mensaje es obligatorio.")]
        string Mensaje
    );
}