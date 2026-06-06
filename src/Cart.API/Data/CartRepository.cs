using Dapper;
using Microsoft.Data.Sqlite;
using CartAPI.Models; 

namespace CartAPI.Data;

public class CartRepository
{
    private readonly IConfiguration _config;

    public CartRepository(IConfiguration config) => _config = config;

    private SqliteConnection CreateConnection() =>
        new(_config.GetConnectionString("DefaultConnection") ?? "Data Source=app.db");

    // ── OBTENER CARRITO ──
    public async Task<CartAPI.Models.Cart?> GetCartByUserIdAsync(Guid userId)
    {
        using var conn = CreateConnection();
       
        var sql = """
            SELECT c.usuario_id AS UsuarioId, 
                   c.fecha_actualizacion AS FechaActualizacion, 
                   ci.producto_id AS ProductoId, 
                   ci.cantidad AS Cantidad 
            FROM carts c
            LEFT JOIN cart_items ci ON c.usuario_id = ci.usuario_id
            WHERE c.usuario_id = @UserId
        """;

        var cartDictionary = new Dictionary<Guid, CartAPI.Models.Cart>();

        await conn.QueryAsync<CartAPI.Models.Cart, CartItem, CartAPI.Models.Cart>(
            sql,
            (cart, cartItem) =>
            {
                if (!cartDictionary.ContainsKey(cart.UsuarioId))
                {
                    cart.Items = new List<CartItem>();
                    cartDictionary.Add(cart.UsuarioId, cart);
                }

                var currentCart = cartDictionary[cart.UsuarioId];

                if (cartItem != null && cartItem.ProductoId != Guid.Empty)
                {
                    currentCart.Items.Add(cartItem);
                }
                return currentCart;
            },
            new { UserId = userId.ToString().ToLower() },
            null, true, "ProductoId" 
        );

        return cartDictionary.Values.FirstOrDefault();
    }

    // ── CREAR O ACTUALIZAR CARRITO ──
    public async Task CreateOrUpdateCartAsync(CartAPI.Models.Cart cart)
    {
        using var conn = CreateConnection();
        await conn.OpenAsync();
        using var transaction = conn.BeginTransaction();

        try
        {
            var sqlCart = """
                INSERT INTO carts (usuario_id, fecha_actualizacion) 
                VALUES (@UsuarioId, @FechaActualizacion)
                ON CONFLICT(usuario_id) DO UPDATE SET 
                fecha_actualizacion = excluded.fecha_actualizacion
            """;

            // CORRECCIÓN 1: Forzamos el ID de usuario a minúscula
            await conn.ExecuteAsync(sqlCart, new
            {
                UsuarioId = cart.UsuarioId.ToString().ToLower(),
                cart.FechaActualizacion
            }, transaction);

            // CORRECCIÓN 2: Forzamos el ID de usuario a minúscula en el DELETE
            await conn.ExecuteAsync(
                "DELETE FROM cart_items WHERE usuario_id = @UsuarioId",
                new { UsuarioId = cart.UsuarioId.ToString().ToLower() }, transaction);

            if (cart.Items != null)
            {
                var sqlItems = """
                    INSERT INTO cart_items (usuario_id, producto_id, cantidad)
                    VALUES (@UsuarioId, @ProductoId, @Cantidad)
                """;

                foreach (var item in cart.Items)
                {
                    // CORRECCIÓN 3: Forzamos ambos IDs a minúscula en el INSERT de ítems
                    await conn.ExecuteAsync(sqlItems, new
                    {
                        UsuarioId = cart.UsuarioId.ToString().ToLower(),
                        ProductoId = item.ProductoId.ToString().ToLower(),
                        item.Cantidad
                    }, transaction);
                }
            }
            transaction.Commit();
        }
        catch
        {
            transaction.Rollback();
            throw;
        }
    }

    // ── ELIMINAR CARRITO ──
    public async Task DeleteCartAsync(Guid userId)
    {
        using var conn = CreateConnection();
        await conn.OpenAsync();
        using var transaction = conn.BeginTransaction();

        try
        {
            // CORRECCIÓN: Usamos cart_items y usuario_id
            await conn.ExecuteAsync(
                "DELETE FROM cart_items WHERE usuario_id = @UserId",
                new { UserId = userId.ToString().ToLower() },
                transaction);

            // CORRECCIÓN: Usamos carts y usuario_id
            await conn.ExecuteAsync(
                "DELETE FROM carts WHERE usuario_id = @UserId",
                new { UserId = userId.ToString().ToLower() },
                transaction);

            transaction.Commit();
        }
        catch
        {
            transaction.Rollback();
            throw;
        }
    }
}