using System.ComponentModel.DataAnnotations;

namespace NotificationsAPI.DTOs
{
    public record CreateNotificationRequest(

        /// <example>a1b2c3d4-0000-0000-0000-111122223333</example>
        [Required(ErrorMessage = "El ID del usuario es obligatorio.")]
        Guid UsuarioId,

        /// <example>Email</example>
        [Required(ErrorMessage = "El tipo de notificación es obligatorio.")]
        string Tipo,

        /// <example>Confirmación de compra</example>
        [Required(ErrorMessage = "El asunto es obligatorio.")]
        string Asunto,

        /// <example>Tu orden ha sido procesada con éxito.</example>
        [Required(ErrorMessage = "El mensaje de la notificación es obligatorio.")]
        string Mensaje
    );
}