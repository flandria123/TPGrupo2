namespace NotificationsAPI.Models
{
    /// <summary>
    /// Representa una notificación a enviar al usuario en la base de datos (SQLite).
    /// </summary>
    public class Notification
    {
        public Guid Id { get; set; }

        
        public Guid UsuarioId { get; set; }

        
        public string Tipo { get; set; } = string.Empty;

       public string Mensaje { get; set; } = string.Empty;

        
        public string Estado { get; set; } = string.Empty;

        public DateTime FechaEnvio { get; set; }
    }
}