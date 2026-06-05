namespace OrdersAPI.DTOs;

public class OrderResponse
{
    public Guid Id { get; set; }
    public Guid UsuarioId { get; set; }
    public decimal Total { get; set; }
    public string Estado { get; set; } = string.Empty;
    public DateTime FechaCreacion { get; set; }

    // Lista con el detalle de los productos comprados
    public List<OrderItemResponse> Items { get; set; } = new();
}

public class OrderItemResponse
{
    public Guid ProductoId { get; set; }
    public int Cantidad { get; set; }
    public decimal PrecioUnitario { get; set; }
}
