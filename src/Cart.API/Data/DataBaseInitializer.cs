using Dapper;
using Microsoft.Data.Sqlite;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory.Database;


namespace CartPI.Data;

public class DatabaseInitializer(IConfiguration config)
{
    private readonly string _connectionString = config.GetConnectionString("DefaultConnection")
                                               ?? "Data Source=app.db";

    public void Initialize()
    {
        using var connection = new SqliteConnection(_connectionString);
        connection.Open();

        // Creamos las tablas siguiendo el modelo del Apéndice A
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
        var count = connection.ExecuteScalar<int>(
            "SELECT COUNT(*) FROM carts");

        if (count > 0)
            return;

        var usuario1 = Guid.Parse("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa");

        var producto1 = Guid.Parse("11111111-1111-1111-1111-111111111111");

        // Crear carrito
        connection.Execute(
            """
        INSERT INTO carts (
            usuario_id,
            fecha_actualizacion
        )
        VALUES (
            @UsuarioId,
            datetime('now')
        )
        """,
            new
            {
                UsuarioId = usuario1.ToString()
            });

        // Crear item
        connection.Execute(
            """
        INSERT INTO cart_items (
            usuario_id,
            producto_id,
            cantidad
        )
        VALUES (
            @UsuarioId,
            @ProductoId,
            @Cantidad
        )
        """,
            new
            {
                UsuarioId = usuario1.ToString(),
                ProductoId = producto1.ToString(),
                Cantidad = 2
            });
    }




}