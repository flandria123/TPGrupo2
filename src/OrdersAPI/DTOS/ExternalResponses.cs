namespace OrdersAPI.DTOs;

// Lo que esperamos que nos responda la API de Usuarios
public class UserExternalResponse
{
    public Guid Id { get; set; }
    public string Estado { get; set; } = string.Empty; // Por si querés validar que no esté bloqueado
}

// Lo que esperamos que nos responda la API de Productos
public class ProductExternalResponse
{
    public Guid Id { get; set; }
    public decimal Precio { get; set; }
    public int Stock { get; set; }
}