using System.ComponentModel.DataAnnotations;

namespace NotificationsAPI.DTOs
{
    /// <summary>
    /// Modelo para enviar una nueva notificación.
    /// </summary>
    public record CreateNotificationRequest
    {
        /// <summary>ID del usuario destinatario.</summary>
        /// <example>21b75cee-f8f6-4261-a370-21b16c40967e</example>
        [Required(ErrorMessage = "El ID del usuario es obligatorio.")]
        public required Guid UsuarioId { get; init; }

        /// <summary>Medio de envío (Email, Push, SMS).</summary>
        /// <example>Email</example>
        [Required(ErrorMessage = "El tipo de notificación es obligatorio.")]
        public required string Tipo { get; init; }

        /// <summary>Contenido de la notificación (máx. 500 caracteres).</summary>
        /// <example>Su orden #f1e2d3c4 fue confirmada.</example>
        [Required(ErrorMessage = "El mensaje de la notificación es obligatorio.")]
        public required string Mensaje { get; init; }
    }
}