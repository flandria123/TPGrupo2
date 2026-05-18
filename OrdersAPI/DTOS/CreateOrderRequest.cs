namespace OrdersAPI.DTOs;

public class CreateOrderRequest
{
    public Guid UsuarioId { get; set; }
    public List<OrderItemRequest> Items { get; set; } = new();
}

public class OrderItemRequest
{
    public Guid ProductoId { get; set; }
    public int Cantidad { get; set; }
}
