namespace CartAPI.DTOs;

public class CartResponse
{
    public Guid UsuarioId { get; set; }

    public List<CartItemResponse> Items { get; set; }
        = new();

    public DateTime FechaActualizacion { get; set; }
}