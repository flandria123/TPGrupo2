using CartAPI.DTOs;
namespace CartAPI.Services
{
    public interface ICartService
    {
        // Obtener carrito del usuario
        Task<CartResponse> GetCartAsync(Guid usuarioId);

        // Agregar producto
        Task<CartResponse> AddItemAsync(
            Guid usuarioId,
            AddItemRequest request);

        // Actualizar cantidad
        Task<CartResponse> UpdateItemAsync(
            Guid usuarioId,
            Guid productoId,
            UpdateItemRequest request);

        // Eliminar producto
        Task RemoveItemAsync(
            Guid usuarioId,
            Guid productoId);

        // Vaciar carrito
        Task ClearCartAsync(Guid usuarioId);
    }
}
