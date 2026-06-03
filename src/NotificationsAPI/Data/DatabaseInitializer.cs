using Dapper;
using Microsoft.Data.Sqlite;

namespace NotificationsAPI.Data
{
    public class DatabaseInitializer
    {
        private readonly IConfiguration _config;
        private readonly ILogger<DatabaseInitializer> _logger;

        public DatabaseInitializer(IConfiguration config, ILogger<DatabaseInitializer> logger)
        {
            _config = config;
            _logger = logger;
        }

        public void Initialize()
        {
            var connectionString = _config.GetConnectionString("DefaultConnection")
                ?? "Data Source=app.db";

            using var connection = new SqliteConnection(connectionString);
            connection.Open();

            // 1. CREACIÓN DE LA TABLA
            connection.Execute("""
            CREATE TABLE IF NOT EXISTS notifications (
                id              TEXT PRIMARY KEY,
                usuario_id      TEXT NOT NULL,
                tipo            TEXT NOT NULL,
                mensaje         TEXT NOT NULL,
                estado          TEXT NOT NULL,
                fecha_envio     TEXT
            );
            """);

            SeedData(connection);

            _logger.LogInformation("SQLite inicializado correctamente → {db}", connectionString);
        }

        private void SeedData(SqliteConnection connection)
        {
            // Verificamos si la tabla ya tiene datos para no duplicarlos
            var count = connection.ExecuteScalar<int>("SELECT COUNT(*) FROM notifications");

            if (count == 0)
            {
                _logger.LogInformation("Insertando notificaciones de prueba...");

                // Usamos un Guid real para que el GuidTypeHandler de Dapper lo intercepte
                var usuarioPruebaId = Guid.Parse("33333333-3333-3333-3333-333333333333");

                connection.Execute("""
                INSERT INTO notifications (id, usuario_id, tipo, mensaje, estado, fecha_envio)
                VALUES (@Id, @UsuarioId, @Tipo, @Mensaje, @Estado, @FechaEnvio)
                """, new[] {
                    new {
                        Id = Guid.NewGuid(), // Se pasa como Guid puro sin el .ToString()
                        UsuarioId = usuarioPruebaId, // Se pasa como Guid puro
                        Tipo = "Email",
                        Mensaje = "Tu nuevo smartphone Motorola ha sido despachado. Recordá realizar la configuración inicial de tu equipo al recibirlo.",
                        Estado = "Enviado",
                        FechaEnvio = (string?)DateTime.UtcNow.ToString("O") // Soluciona la advertencia verde
                    },
                    new {
                        Id = Guid.NewGuid(),
                        UsuarioId = usuarioPruebaId,
                        Tipo = "Push",
                        Mensaje = "Hemos detectado un nuevo inicio de sesión en tu cuenta desde un dispositivo no reconocido.",
                        Estado = "Pendiente",
                        FechaEnvio = (string?)null
                    }
                });
            }
        }
    }
}