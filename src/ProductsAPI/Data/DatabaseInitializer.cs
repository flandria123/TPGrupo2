using Dapper;
using Microsoft.Data.Sqlite;

namespace ProductsAPI.Data
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

            // 1. CREACIÓN DE LA TABLA BASADA EN EL APÉNDICE A
            connection.Execute("""
            CREATE TABLE IF NOT EXISTS products (
                id              TEXT PRIMARY KEY,
                nombre          TEXT NOT NULL,
                descripcion     TEXT,
                precio          REAL NOT NULL,
                stock           INTEGER NOT NULL,
                categoria       TEXT NOT NULL,
                fecha_creacion  TEXT NOT NULL DEFAULT (datetime('now'))
            );
            """);

            // 2. INSERCIÓN DE DATOS DE PRUEBA
            SeedData(connection);

            _logger.LogInformation("SQLite inicializado correctamente → {db}", connectionString);
        }

        private void SeedData(SqliteConnection connection)
        {
            var count = connection.ExecuteScalar<int>("SELECT COUNT(*) FROM products");

            if (count == 0)
            {
                _logger.LogInformation("Insertando productos de prueba en el catálogo...");

                connection.Execute("""
                INSERT INTO products (id, nombre, descripcion, precio, stock, categoria, fecha_creacion)
                VALUES (@Id, @Nombre, @Descripcion, @Precio, @Stock, @Categoria, @FechaCreacion)
                """, new[] {
                    new {
                        // ID obligatorio de la documentación de la cátedra
                        Id = Guid.Parse("3fa85f64-5717-4562-b3fc-2c963f66afa6"),
                        Nombre = "Notebook Dell XPS 15",
                        Descripcion = "Laptop 15 pulgadas, 32GB RAM",
                        Precio = 1500.00m,
                        Stock = 10,
                        Categoria = "Electrónica",
                        FechaCreacion = "2024-01-15T10:30:00Z"
                    },
                    new {
                        Id = Guid.Parse("aaaabbbb-cccc-dddd-eeee-ffff00001111"),
                        Nombre = "Silla de Oficina Ergonómica",
                        Descripcion = "Silla con soporte lumbar y apoyabrazos ajustables",
                        Precio = 120.50m,
                        Stock = 5,
                        Categoria = "Hogar y Deco",
                        FechaCreacion = DateTime.UtcNow.ToString("O")
                    },
                    new {
                        Id = Guid.Parse("11112222-3333-4444-5555-666677778888"),
                        Nombre = "Motorola Moto G84",
                        Descripcion = "Smartphone 8GB RAM, 256GB, NFC habilitado",
                        Precio = 350.00m,
                        Stock = 15,
                        Categoria = "Smartphones",
                        FechaCreacion = DateTime.UtcNow.ToString("O")
                    },
                    new {
                        Id = Guid.Parse("99998888-7777-6666-5555-444433332222"),
                        Nombre = "Bloodborne - Game of the Year Edition (PS4)",
                        Descripcion = "Juego Físico, 100% Offline. Edición completa.",
                        Precio = 25.99m,
                        Stock = 8,
                        Categoria = "Videojuegos",
                        FechaCreacion = DateTime.UtcNow.ToString("O")
                    },
                    new {
                        Id = Guid.Parse("abcdef12-3456-7890-abcd-ef1234567890"),
                        Nombre = "Libro: SQL Server y Arquitectura de Microservicios",
                        Descripcion = "Guía avanzada de bases de datos relacionales y C#",
                        Precio = 45.00m,
                        Stock = 20,
                        Categoria = "Libros",
                        FechaCreacion = DateTime.UtcNow.ToString("O")
                    }
                });
            }
        }
    }
}