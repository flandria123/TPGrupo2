using Microsoft.Data.Sqlite;
using Dapper;

namespace Users.API.Data
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

            // En caso de tener que agregar otra tabla es duplicar este comando
            connection.Execute("""
            CREATE TABLE IF NOT EXISTS items (
                Id          INTEGER PRIMARY KEY AUTOINCREMENT,
                Nombre      TEXT NOT NULL,
                Apellido    TEXT NOT NULL,
                Email       TEXT NOT NULL UNIQUE,
                Password    TEXT NOT NULL,
                FechaRegistro TEXT NOT NULL DEFAULT (datetime('now')),
                Activo      INTEGER NOT NULL DEFAULT 1,
                IntentosFallidos INTEGER NOT NULL DEFAULT 0
            );
        """);



            _logger.LogInformation("SQLite inicializado correctamente → {db}", connectionString);
        }

    }
}
