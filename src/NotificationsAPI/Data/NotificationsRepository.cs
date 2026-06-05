using Dapper;
using Microsoft.Data.Sqlite;
using NotificationsAPI.Models;

namespace NotificationsAPI.Data
{
    public class NotificationsRepository
    {
        private readonly IConfiguration _config;

        public NotificationsRepository(IConfiguration config) => _config = config;

        private SqliteConnection CreateConnection() =>
            new(_config.GetConnectionString("DefaultConnection") ?? "Data Source=app.db");

        // ── CREATE (Para registrar la notificación cuando ingresa el POST) ────────────
        public async Task<Notification> CreateAsync(Notification notification)
        {
            using var conn = CreateConnection();

            await conn.ExecuteAsync("""
                INSERT INTO notifications (id, usuario_id, mensaje, tipo, estado, fecha_envio)
            VALUES (@Id, @UsuarioId, @Mensaje, @Tipo, @Estado, @FechaEnvio);
         """, notification);


            return notification;
        }

        // ── GET BY ID (Crucial para buscar la entidad antes de actualizar su estado) ──
        public async Task<Notification?> GetByIdAsync(Guid id)
        {
            using var conn = CreateConnection();

            // Usamos AS para mapear el snake_case de SQLite al PascalCase de C#
            return await conn.QuerySingleOrDefaultAsync<Notification>("""
                SELECT id AS Id, 
                       usuario_id AS UsuarioId, 
                       mensaje AS Mensaje, 
                       tipo AS Tipo, 
                       estado AS Estado, 
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
                       mensaje AS Mensaje, 
                       tipo AS Tipo, 
                       estado AS Estado, 
                       fecha_envio AS FechaEnvio
                FROM notifications
                WHERE usuario_id = @usuarioId
                ORDER BY fecha_envio DESC
            """, new { usuarioId });
        }

        

             


    }
}