using Dapper;
using Microsoft.Data.Sqlite;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace OrdersAPI.Data
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

            // 1. CREACIÓN DE TABLAS (Cabecera y Detalle)
            // Se utiliza TEXT para los IDs porque SQLite no tiene tipo GUID nativo
            connection.Execute("""
            CREATE TABLE IF NOT EXISTS orders (
                id              TEXT PRIMARY KEY,
                usuario_id      TEXT NOT NULL,
                total           REAL NOT NULL,
                estado          TEXT NOT NULL,
                fecha_creacion  TEXT NOT NULL
            );

            CREATE TABLE IF NOT EXISTS order_items (
                order_id        TEXT NOT NULL,
                producto_id     TEXT NOT NULL,
                cantidad        INTEGER NOT NULL,
                precio_unitario REAL NOT NULL,
                FOREIGN KEY (order_id) REFERENCES orders(id) ON DELETE CASCADE
            );
            """);

            // 2. INYECCIÓN DE DATOS DE PRUEBA (Seed Data)
            // Verificamos si la tabla ya tiene datos para no duplicarlos cada vez que arranca la API
            var orderCount = connection.ExecuteScalar<int>("SELECT COUNT(*) FROM orders");

            if (orderCount == 0)
            {
                _logger.LogInformation("Inyectando datos de prueba en la base de datos...");

                // Usamos los IDs exactos de los ejemplos del TP para que coincidan con Users y Products API
                var orderId = "f1e2d3c4-0000-0000-0000-aabbccddeeff"; // ID de Orden del TP
                var usuarioId = "21b75cee-f8f6-4261-a370-21b16c40967e"; // ID Tomas
                var productoId = "3fa85f64-5717-4562-b3fc-2c963f66afa6"; // ID Notebook Dell XPS 15

                connection.Execute("""
                INSERT INTO orders (id, usuario_id, total, estado, fecha_creacion)
                VALUES (@Id, @UsuarioId, @Total, @Estado, @FechaCreacion);
                """, new
                {
                    Id = orderId,
                    UsuarioId = usuarioId,
                    Total = 3000.00m,
                    Estado = "Pendiente",
                    FechaCreacion = DateTime.UtcNow.ToString("O")
                });

                connection.Execute("""
                INSERT INTO order_items (order_id, producto_id, cantidad, precio_unitario)
                VALUES (@OrderId, @ProductoId, @Cantidad, @PrecioUnitario);
                """, new
                {
                    OrderId = orderId,
                    ProductoId = productoId,
                    Cantidad = 2,
                    PrecioUnitario = 1500.00m
                });
            }

            _logger.LogInformation("SQLite inicializado correctamente → {db}", connectionString);
        }
    }
}