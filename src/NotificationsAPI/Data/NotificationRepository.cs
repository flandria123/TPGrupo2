using Dapper;
using Microsoft.Data.Sqlite;
using NotificationsAPI.Models;

namespace NotificationsAPI.Data
{
    public class NotificationRepository
    {
        private readonly IConfiguration _config;

        public NotificationRepository(IConfiguration config) => _config = config;

        private SqliteConnection CreateConnection() =>
            new(_config.GetConnectionString("DefaultConnection") ?? "Data Source=app.db");

        // ── GET BY ID (Crucial para buscar la entidad antes de actualizar su estado) ──
        public async Task<Notification?> GetByIdAsync(Guid id)
        {
            using var conn = CreateConnection();

            // Usamos AS para mapear el snake_case de SQLite al PascalCase de C#
            return await conn.QuerySingleOrDefaultAsync<Notification>("""
                SELECT id AS Id, 
                       usuario_id AS UsuarioId, 
                       tipo AS Tipo, 
                       asunto AS Asunto, 
                       mensaje AS Mensaje, 
                       estado AS Estado, 
                       fecha_creacion AS FechaCreacion, 
                       fecha_envio AS FechaEnvio
                FROM notifications
                WHERE id = @id
            """, new { id });
        }

        // ── GET BY USUARIO ID (Para que el usuario pueda ver su historial de alertas) ──
        public async Task<IEnumerable<Notification>> GetAllByUsuarioIdAsync(Guid usuarioId)
        {
            using var conn = CreateConnection();

            return await conn.QueryAsync<Notification>("""
                SELECT id AS Id, 
                       usuario_id AS UsuarioId, 
                       tipo AS Tipo, 
                       asunto AS Asunto, 
                       mensaje AS Mensaje, 
                       estado AS Estado, 
                       fecha_creacion AS FechaCreacion, 
                       fecha_envio AS FechaEnvio
                FROM notifications
                WHERE usuario_id = @usuarioId
                ORDER BY fecha_creacion DESC
            """, new { usuarioId });
        }

        // ── CREATE (Para registrar la notificación cuando ingresa el POST) ────────────
        public async Task<Notification> CreateAsync(Notification notification)
        {
            using var conn = CreateConnection();

            await conn.ExecuteAsync("""
                INSERT INTO notifications (id, usuario_id, tipo, asunto, mensaje, estado, fecha_creacion, fecha_envio)
                VALUES (@Id, @UsuarioId, @Tipo, @Asunto, @Mensaje, @Estado, @FechaCreacion, @FechaEnvio);
            """, notification);

            return notification;
        }

        // ── UPDATE (Para actualizar el Estado, el Mensaje o la Fecha de Envío) ────────
        public async Task<bool> UpdateAsync(Notification notification)
        {
            using var conn = CreateConnection();

            // Solo actualizamos los campos mutables definidos en nuestra lógica de negocio
            var rows = await conn.ExecuteAsync("""
                UPDATE notifications
                SET estado = @Estado,
                    mensaje = @Mensaje,
                    fecha_envio = @FechaEnvio
                WHERE id = @Id
            """, new
            {
                notification.Estado,
                notification.Mensaje,
                notification.FechaEnvio,
                notification.Id
            });

            return rows > 0;
        }
    }
}