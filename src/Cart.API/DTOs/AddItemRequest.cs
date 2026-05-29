namespace CartAPI.DTOs;

public class AddItemRequest
{
    public Guid ProductoId { get; set; }

    public int Cantidad { get; set; }
}