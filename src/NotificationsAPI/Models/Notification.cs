namespace NotificationsAPI.Models
{
    /// <summary>
    /// Representa una notificación a enviar al usuario en la base de datos (SQLite).
    /// </summary>
    public class Notification
    {
        public Guid Id { get; set; }

        // Identificador del usuario destino (conectado con la Users API)
        public Guid UsuarioId { get; set; }

        // Ej: "Email", "SMS"
        public string Tipo { get; set; } = string.Empty;

        public string Asunto { get; set; } = string.Empty;

        public string Mensaje { get; set; } = string.Empty;

        // Ej: "Pendiente", "Enviado", "Fallido"
        public string Estado { get; set; } = string.Empty;

        public DateTime FechaCreacion { get; set; }

        public DateTime? FechaEnvio { get; set; }
    }
}