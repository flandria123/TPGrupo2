using OrdersAPI.DTOs;

namespace OrdersAPI.Services;

public interface IOrderService
{
    // Ahora devolvemos DTOs (OrderResponse) en lugar del modelo Order
    Task<IEnumerable<OrderResponse>> GetOrdersAsync(Guid? usuarioId);
    Task<OrderResponse> GetOrderByIdAsync(Guid id);
    Task<OrderResponse> CreateOrderAsync(CreateOrderRequest request);

    // UpdateStatus no devuelve nada (void asincrónico), así que se mantiene igual
    Task UpdateStatusAsync(Guid id, string nuevoEstado);
}