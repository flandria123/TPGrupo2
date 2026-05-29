using CartAPI.Models;
using Dapper;
using Microsoft.Data.Sqlite;

namespace CartAPI.Data
{
    public class CartRepository
    {
        private readonly IConfiguration _config;
        private readonly string _connectionString;

        public CartRepository(IConfiguration config)
        {
            _config = config;

            _connectionString =
                _config.GetConnectionString("DefaultConnection")
                ?? "Data Source=app.db";
        }

        // ──────────────────────────────────────────────────────────────────────────
        // OBTENER CARRITO POR USUARIO
        // ──────────────────────────────────────────────────────────────────────────
        public async Task<Cart?> GetByUserIdAsync(Guid usuarioId)
        {
            using var connection =
                new SqliteConnection(_connectionString);

            connection.Open();

            // Buscar cabecera carrito
            var cart = await connection.QueryFirstOrDefaultAsync<Cart>(
                """
            SELECT
                usuario_id AS UsuarioId,
                fecha_actualizacion AS FechaActualizacion
            FROM carts
            WHERE usuario_id = @UsuarioId
            """,
                new
                {
                    UsuarioId = usuarioId.ToString()
                });

            if (cart == null)
                return null;

            // Buscar items del carrito
            var items = await connection.QueryAsync<CartItem>(
                """
            SELECT
                producto_id AS ProductoId,
                cantidad AS Cantidad
            FROM cart_items
            WHERE usuario_id = @UsuarioId
            """,
                new
                {
                    UsuarioId = usuarioId.ToString()
                });

            cart.Items = items.ToList();

            return cart;
        }

        // ──────────────────────────────────────────────────────────────────────────
        // CREAR CARRITO
        // ──────────────────────────────────────────────────────────────────────────
        public async Task CreateAsync(Cart cart)
        {
            using var connection =
                new SqliteConnection(_connectionString);

            connection.Open();

            // Insertar cabecera carrito
            await connection.ExecuteAsync(
                """
            INSERT INTO carts (
                usuario_id,
                fecha_actualizacion
            )
            VALUES (
                @UsuarioId,
                @FechaActualizacion
            )
            """,
                new
                {
                    UsuarioId = cart.UsuarioId.ToString(),
                    FechaActualizacion = cart.FechaActualizacion
                });

            // Insertar items
            foreach (var item in cart.Items)
            {
                await connection.ExecuteAsync(
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
                        UsuarioId = cart.UsuarioId.ToString(),
                        ProductoId = item.ProductoId.ToString(),
                        Cantidad = item.Cantidad
                    });
            }
        }

        // ──────────────────────────────────────────────────────────────────────────
        // ACTUALIZAR CARRITO
        // ──────────────────────────────────────────────────────────────────────────
        public async Task UpdateAsync(Cart cart)
        {
            using var connection =
                new SqliteConnection(_connectionString);

            connection.Open();

            // Actualizar fecha carrito
            await connection.ExecuteAsync(
                """
            UPDATE carts
            SET fecha_actualizacion = @FechaActualizacion
            WHERE usuario_id = @UsuarioId
            """,
                new
                {
                    UsuarioId = cart.UsuarioId.ToString(),
                    FechaActualizacion = cart.FechaActualizacion
                });

            // Estrategia simple para el TP:
            // borrar todos los items y recrearlos

            await connection.ExecuteAsync(
                """
            DELETE FROM cart_items
            WHERE usuario_id = @UsuarioId
            """,
                new
                {
                    UsuarioId = cart.UsuarioId.ToString()
                });

            // Reinsertar items actualizados
            foreach (var item in cart.Items)
            {
                await connection.ExecuteAsync(
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
                        UsuarioId = cart.UsuarioId.ToString(),
                        ProductoId = item.ProductoId.ToString(),
                        Cantidad = item.Cantidad
                    });
            }


        }

        


    }
}
