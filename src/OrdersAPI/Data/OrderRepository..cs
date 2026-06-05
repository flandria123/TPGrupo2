using Dapper;
using Microsoft.Data.Sqlite;
using Microsoft.Extensions.Configuration;
using OrdersAPI.Models;

namespace OrdersAPI.Data
{
    public class OrderRepository
    {
        private readonly IConfiguration _config;

        public OrderRepository(IConfiguration config) => _config = config;

        private SqliteConnection CreateConnection() =>
            new(_config.GetConnectionString("DefaultConnection") ?? "Data Source=app.db");

        // ── GET ALL (Listar órdenes con filtro opcional) ──────────────────────
        public async Task<IEnumerable<Order>> GetAllAsync(Guid? usuarioId)
        {
            using var conn = CreateConnection();

            var sql = @"
                SELECT id AS Id, usuario_id AS UsuarioId, total AS Total, 
                       estado AS Estado, fecha_creacion AS FechaCreacion
                FROM orders";

            if (usuarioId.HasValue)
            {
                sql += " WHERE usuario_id = @UsuarioId";
            }

            sql += " ORDER BY fecha_creacion DESC";

            // Listamos las cabeceras
            var orders = (await conn.QueryAsync<Order>(sql, new { UsuarioId = usuarioId })).ToList();

            // Para cada orden, buscamos sus items (idealmente se hace con un JOIN, 
            // pero esto mantiene la simplicidad del código de los profesores)
            foreach (var order in orders)
            {
                order.Items = (await GetItemsByOrderIdAsync(order.Id, conn)).ToList();
            }

            return orders;
        }

        // ── GET BY ID (Obtener detalle completo de una orden) ─────────────────
        public async Task<Order?> GetByIdAsync(Guid id)
        {
            using var conn = CreateConnection();

            var order = await conn.QuerySingleOrDefaultAsync<Order>(@"
                SELECT id AS Id, usuario_id AS UsuarioId, total AS Total, 
                       estado AS Estado, fecha_creacion AS FechaCreacion
                FROM orders
                WHERE id = @Id", new { Id = id });

            if (order != null)
            {
                order.Items = (await GetItemsByOrderIdAsync(order.Id, conn)).ToList();
            }

            return order;
        }

        // ── CREATE (Guardar orden y sus items con Transacción) ────────────────
        public async Task AddAsync(Order order)
        {
            using var conn = CreateConnection();
            await conn.OpenAsync();

            // Iniciamos transacción para asegurar consistencia de datos
            using var transaction = conn.BeginTransaction();

            try
            {
                // 1. Insertar Cabecera
                await conn.ExecuteAsync(@"
                    INSERT INTO orders (id, usuario_id, total, estado, fecha_creacion)
                    VALUES (@Id, @UsuarioId, @Total, @Estado, @FechaCreacion)",
                    new
                    {
                        Id = order.Id.ToString(), // SQLite guarda GUIDs como texto
                        UsuarioId = order.UsuarioId.ToString(),
                        order.Total,
                        order.Estado,
                        FechaCreacion = order.FechaCreacion.ToString("O")
                    }, transaction);

                // 2. Insertar Detalle (Items)
                foreach (var item in order.Items)
                {
                    await conn.ExecuteAsync(@"
                        INSERT INTO order_items (order_id, producto_id, cantidad, precio_unitario)
                        VALUES (@OrderId, @ProductoId, @Cantidad, @PrecioUnitario)",
                        new
                        {
                            OrderId = order.Id.ToString(),
                            ProductoId = item.ProductoId.ToString(),
                            item.Cantidad,
                            item.PrecioUnitario
                        }, transaction);
                }

                // Si todo sale bien, confirmamos los cambios
                transaction.Commit();
            }
            catch
            {
                // Si algo falla, revertimos para no dejar una orden sin items
                transaction.Rollback();
                throw;
            }
        }

        // ── UPDATE (Actualizar estado de la orden) ────────────────────────────
        public async Task UpdateAsync(Order order)
        {
            using var conn = CreateConnection();

            await conn.ExecuteAsync(@"
                UPDATE orders
                SET estado = @Estado
                WHERE id = @Id",
                new
                {
                    order.Estado,
                    Id = order.Id.ToString()
                });
        }

        // ── HELPER PRIVADO: Obtener items de una orden ────────────────────────
        private async Task<IEnumerable<OrderItem>> GetItemsByOrderIdAsync(Guid orderId, SqliteConnection conn)
        {
            return await conn.QueryAsync<OrderItem>(@"
                SELECT producto_id AS ProductoId, cantidad AS Cantidad, precio_unitario AS PrecioUnitario
                FROM order_items
                WHERE order_id = @OrderId",
                new { OrderId = orderId.ToString() });
        }
    }
}