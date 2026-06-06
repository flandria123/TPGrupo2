using OrdersAPI.DTOs;
using OrdersAPI.DTOS;

namespace OrdersAPI.Services;

public interface IOrderService
{
    // Ahora devolvemos DTOs (OrderResponse) en lugar del modelo Order
    Task<IEnumerable<OrderResponse>> GetOrdersAsync(Guid? usuarioId);
    Task<OrderResponse> GetOrderByIdAsync(Guid id);
    Task<OrderResponse> CreateOrderAsync(CreateOrderRequest request);



   
    Task<UpdateStatusResponse> UpdateStatusAsync(Guid id, string estado);


}