namespace OrdersAPI.Models;

public class Order
{
    public Guid Id { get; set; } // Identificador único [cite: 323]
    public Guid UsuarioId { get; set; } // Referencia al usuario [cite: 323]
    public List<OrderItem> Items { get; set; } = new(); // Lista de productos [cite: 323]
    public decimal Total { get; set; } // Calculado automáticamente [cite: 323]
    public string Estado { get; set; } // Pendiente | Confirmada | Enviada | Entregada | Cancelada [cite: 323]
    public DateTime FechaCreacion { get; set; } // Asignado automáticamente [cite: 323]
}