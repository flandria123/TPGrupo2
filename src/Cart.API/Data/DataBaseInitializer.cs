using Dapper;
using Microsoft.Data.Sqlite;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory.Database;


namespace CartPI.Data
{
    public class DatabaseInitializer(IConfiguration config)
    {
        private readonly string _connectionString = config.GetConnectionString("DefaultConnection")
                                                   ?? "Data Source=app.db";

        public void Initialize()
        {
            using var connection = new SqliteConnection(_connectionString);
            connection.Open();

            // Creción de las tablas siguiendo el modelo 
            connection.Execute("""
            -- Tabla de cabecera del carrito
            CREATE TABLE IF NOT EXISTS carts (
                usuario_id          TEXT PRIMARY KEY, -- Guid como string
                fecha_actualizacion TEXT NOT NULL DEFAULT (datetime('now'))
            );

            -- Tabla de ítems del carrito
            CREATE TABLE IF NOT EXISTS cart_items (
                usuario_id  TEXT NOT NULL,
                producto_id TEXT NOT NULL, -- Guid como string
                cantidad    INTEGER NOT NULL CHECK(cantidad > 0), -- Validación CRT-004
                PRIMARY KEY (usuario_id, producto_id),
                FOREIGN KEY (usuario_id) REFERENCES carts(usuario_id) ON DELETE CASCADE
            );
        """);

            SeedData(connection);


        }


        private void SeedData(SqliteConnection connection)
        {
            // 1. Verificamos si ya hay datos para no duplicar
            var count = connection.ExecuteScalar<int>("SELECT COUNT(*) FROM carts");
            if (count > 0) return;

            // 2. Definimos IDs fijos ( mismos que en Products y Users )
            var usuarioId = "21b75cee-f8f6-4261-a370-21b16c40967e";
            var productoId1 = "3fa85f64-5717-4562-b3fc-2c963f66afa6"; // Ej: Notebook
            var productoId2 = "abcdef12-3456-7890-abcd-ef1234567890"; // Ej: Libro

            // 3. Insertamos la cabecera del carrito
            connection.Execute("""
        INSERT INTO carts (usuario_id, fecha_actualizacion)
        VALUES (@UsuarioId, datetime('now'))
    """, new { UsuarioId = usuarioId });

            // 4. Insertamos múltiples ítems para probar la lista
            connection.Execute("""
        INSERT INTO cart_items (usuario_id, producto_id, cantidad)
        VALUES (@UsuarioId, @ProductoId, @Cantidad)
    """, new[] {
        new { UsuarioId = usuarioId, ProductoId = productoId1, Cantidad = 1 },
        new { UsuarioId = usuarioId, ProductoId = productoId2, Cantidad = 3 }
    });


        }
    }

}


