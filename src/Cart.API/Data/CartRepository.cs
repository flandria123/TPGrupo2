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
            SELECT c.UsuarioId, c.FechaActualizacion, 
                   ci.ProductoId, ci.Cantidad 
            FROM Carts c
            LEFT JOIN CartItems ci ON c.UsuarioId = ci.UsuarioId
            WHERE c.UsuarioId = @UserId
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
            new { UserId = userId },
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
                INSERT INTO Carts (UsuarioId, FechaActualizacion) 
                VALUES (@UsuarioId, @FechaActualizacion)
                ON CONFLICT(UsuarioId) DO UPDATE SET 
                FechaActualizacion = excluded.FechaActualizacion
            """;

            await conn.ExecuteAsync(sqlCart, new { cart.UsuarioId, cart.FechaActualizacion }, transaction);

            await conn.ExecuteAsync(
                "DELETE FROM CartItems WHERE UsuarioId = @UsuarioId",
                new { cart.UsuarioId }, transaction);

            if (cart.Items != null)
            {
                var sqlItems = """
                    INSERT INTO CartItems (UsuarioId, ProductoId, Cantidad)
                    VALUES (@UsuarioId, @ProductoId, @Cantidad)
                """;

                foreach (var item in cart.Items)
                {
                    await conn.ExecuteAsync(sqlItems, new
                    {
                        cart.UsuarioId,
                        item.ProductoId,
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
            await conn.ExecuteAsync(
                "DELETE FROM CartItems WHERE UsuarioId = @UserId",
                new { UserId = userId },
                transaction); // <-- Agregamos la transacción

            await conn.ExecuteAsync(
                "DELETE FROM Carts WHERE UsuarioId = @UserId",
                new { UserId = userId },
                transaction); // <-- Agregamos la transacción

            transaction.Commit();
        }
        catch
        {
            transaction.Rollback();
            throw;
        }
    }
}