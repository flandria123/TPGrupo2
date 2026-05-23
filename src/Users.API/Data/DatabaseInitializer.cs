using Dapper;
using Microsoft.AspNetCore.Identity;
using Microsoft.Data.Sqlite;
using static System.Runtime.InteropServices.JavaScript.JSType;

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
            CREATE TABLE IF NOT EXISTS usuarios (
                id          TEXT PRIMARY KEY,
                nombre      TEXT NOT NULL,
                apellido    TEXT NOT NULL,
                email       TEXT NOT NULL UNIQUE,
                password_hash TEXT NOT NULL,
                fecha_registro TEXT NOT NULL DEFAULT (datetime('now')),
                activo      INTEGER NOT NULL DEFAULT 1,
                intentos_fallidos INTEGER NOT NULL DEFAULT 0
            );
        """);

            SeedData(connection);

            _logger.LogInformation("SQLite inicializado correctamente → {db}", connectionString);
        }

        private void SeedData(SqliteConnection connection)
        {
            // Verificamos si la tabla ya tiene datos para no duplicarlos [1, 4]
            var count = connection.ExecuteScalar<int>("SELECT COUNT(*) FROM usuarios");

            if (count == 0)
            {
                _logger.LogInformation("Insertando usuarios de prueba...");

                // Insertamos al menos un usuario activo y uno bloqueado para la defensa [5-7]
                connection.Execute("""
                INSERT INTO usuarios (id, nombre, apellido, email, password_hash, activo, intentos_fallidos)
                VALUES (@Id, @Nombre, @Apellido, @Email, @PasswordHash, @Activo, @IntentosFallidos)
            """,    new[] {
                    new {
                        Id = Guid.NewGuid(),
                        Nombre = "Tomas", Apellido = "Ponti",
                        Email = "tomasP@gmail.com",
                        PasswordHash = BCrypt.Net.BCrypt.HashPassword("Admin123!"),
                        Activo = true, IntentosFallidos = 0
                    },
                    new {
                        Id = Guid.NewGuid(),
                        Nombre = "Tomas", Apellido = "Ustimczuk",
                        Email = "tomasU@gmail.com",
                        PasswordHash = BCrypt.Net.BCrypt.HashPassword("User123!"),
                        Activo = false, IntentosFallidos = 3
                    },
                    new {
                        Id = Guid.NewGuid(),
                        Nombre = "Christian", Apellido = "Jackson",
                        Email = "christian@gmail.com",
                        PasswordHash = BCrypt.Net.BCrypt.HashPassword("User123!"),
                        Activo = true, IntentosFallidos = 2
                    },
                    new {
                        Id = Guid.NewGuid(),
                        Nombre = "Cosme", Apellido = "Fulanito",
                        Email = "cosmefulanito@gmail.com",
                        PasswordHash = BCrypt.Net.BCrypt.HashPassword("User123!"),
                        Activo = false, IntentosFallidos = 1
                    },
                });
            }
        }
    }
}