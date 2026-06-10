namespace NotificationsAPI.DTOs
{
    /// <summary>
    /// Representa los datos de una notificación devuelta al cliente.
    /// </summary>
    public record NotificationResponse
    {
        /// <summary>ID único de la notificación.</summary>
        /// <example>11112222-3333-4444-5555-666677778888</example>
        public required Guid Id { get; init; }

        /// <summary>ID del usuario destinatario.</summary>
        /// <example>21b75cee-f8f6-4261-a370-21b16c40967e</example>
        public required Guid UsuarioId { get; init; }

        /// <summary>Medio de envío utilizado.</summary>
        /// <example>Email</example>
        public required string Tipo { get; init; }

        /// <summary>Contenido de la notificación.</summary>
        /// <example>Su orden #f1e2d3c4 fue confirmada.</example>
        public required string Mensaje { get; init; }

        /// <summary>Estado actual del envío (Pendiente, Enviada, Fallida).</summary>
        /// <example>Enviada</example>
        public required string Estado { get; init; }

        /// <summary>Fecha en la que se procesó el envío.</summary>
        /// <example>2024-03-10T12:01:00Z</example>
        public required DateTime FechaEnvio { get; init; }
    }
}