namespace NotificationsAPI.DTOs
{
    /// <summary>
    /// Representa los datos de una notificación devuelta al cliente.
    /// </summary>
    public record NotificationResponse(

        /// <example>f5a1b2c3-4d5e-6f7a-8b9c-0d1e2f3a4b5c</example>
        Guid Id,

        /// <example>a1b2c3d4-0000-0000-0000-111122223333</example>
        Guid UsuarioId,

        /// <example>Email</example>
        string Tipo,

        /// <example>Confirmación de compra</example>
        string Asunto,

        /// <example>Tu orden ha sido procesada con éxito.</example>
        string Mensaje,

        /// <example>Enviado</example>
        string Estado,

        /// <example>2026-06-03T14:30:00Z</example>
        DateTime FechaCreacion,

        /// <example>2026-06-03T14:30:05Z</example>
        DateTime? FechaEnvio
    );
}