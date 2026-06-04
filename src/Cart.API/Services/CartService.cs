using System.Net.Http.Json;
using CartAPI.DTOs;
using CartAPI.Exceptions;
using CartAPI.Models;
using CartAPI.Data;

namespace CartAPI.Services
{
    
       public class CartService : ICartService
    {
        private readonly CartRepository _cartRepository;
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly ILogger<CartService> _logger;

        public CartService(CartRepository cartRepository, IHttpClientFactory httpClientFactory, ILogger<CartService> logger)
        {
            _cartRepository = cartRepository;
            _httpClientFactory = httpClientFactory;
            _logger = logger;
        }

        // ── 1. OBTENER CARRITO (GET) ──
        public async Task<CartResponse> GetCartAsync(Guid usuarioId)
        {
            _logger.LogInformation("Buscando carrito para el usuario {UserId}", usuarioId);

            var cart = await _cartRepository.GetCartByUserIdAsync(usuarioId);

            // VALIDACIÓN CRT-001: Carrito no encontrado
            if (cart == null || !cart.Items.Any())
            {
                _logger.LogWarning("Carrito no encontrado o vacío para el usuario {UserId}", usuarioId);
                throw new NotFoundException("CRT-001", "Carrito no encontrado.");
            }

            _logger.LogInformation("Carrito obtenido exitosamente para el usuario {UserId}", usuarioId);
            return MapToResponse(cart);
        }

        // ── 2. AGREGAR ITEM (POST) ──
        public async Task<CartResponse> AddItemAsync(Guid usuarioId, AddItemRequest request)
        {
            _logger.LogInformation("Intentando agregar {Cantidad} unidades del producto {ProductoId} al carrito {UserId}",
                request.Cantidad, request.ProductoId, usuarioId);

            var cart = await _cartRepository.GetCartByUserIdAsync(usuarioId)
                       ?? new CartAPI.Models.Cart { UsuarioId = usuarioId, Items = new() };

            var itemExistente = cart.Items.FirstOrDefault(i => i.ProductoId == request.ProductoId);
            int cantidadActualEnCarrito = itemExistente?.Cantidad ?? 0;

            // Validamos que exista y tenga stock en Products.API
            await ValidateProductStockAsync(request.ProductoId, request.Cantidad, cantidadActualEnCarrito);

            if (itemExistente != null)
            {
                itemExistente.Cantidad += request.Cantidad;
            }
            else
            {
                cart.Items.Add(new CartAPI.Models.CartItem { ProductoId = request.ProductoId, Cantidad = request.Cantidad });
            }

            cart.FechaActualizacion = DateTime.UtcNow;
            await _cartRepository.CreateOrUpdateCartAsync(cart);

            _logger.LogInformation("Producto {ProductoId} agregado exitosamente. Total en carrito: {TotalItem}",
                request.ProductoId, itemExistente?.Cantidad ?? request.Cantidad);

            return MapToResponse(cart);
        }

        // ── 3. ACTUALIZAR ITEM (PUT) ──
        public async Task<CartResponse> UpdateItemAsync(Guid usuarioId, Guid productoId, UpdateItemRequest request)
        {
            _logger.LogInformation("Actualizando cantidad del producto {ProductoId} a {NuevaCantidad} en carrito {UserId}",
                productoId, request.Cantidad, usuarioId);

            var cart = await _cartRepository.GetCartByUserIdAsync(usuarioId);

            // VALIDACIÓN CRT-001
            if (cart == null || !cart.Items.Any())
            {
                _logger.LogWarning("Intento de actualizar item en carrito inexistente. Usuario: {UserId}", usuarioId);
                throw new NotFoundException("CRT-001", "Carrito no encontrado.");
            }

            var itemExistente = cart.Items.FirstOrDefault(i => i.ProductoId == productoId);

            // VALIDACIÓN CRT-002: Producto no encontrado en el carrito
            if (itemExistente == null)
            {
                _logger.LogWarning("El producto {ProductoId} no se encuentra en el carrito del usuario {UserId}", productoId, usuarioId);
                throw new NotFoundException("CRT-002", "Producto no encontrado.");
            }

            // Validamos stock pasándole 0 en 'cantidadActualEnCarrito' porque el PUT reemplaza la cantidad, no suma
            await ValidateProductStockAsync(productoId, request.Cantidad, 0);

            itemExistente.Cantidad = request.Cantidad;
            cart.FechaActualizacion = DateTime.UtcNow;
            await _cartRepository.CreateOrUpdateCartAsync(cart);

            _logger.LogInformation("Cantidad actualizada exitosamente para el producto {ProductoId}", productoId);
            return MapToResponse(cart);
        }

        // ── 4. ELIMINAR UN ITEM (DELETE) ──
        public async Task RemoveItemAsync(Guid usuarioId, Guid productoId)
        {
            _logger.LogInformation("Eliminando producto {ProductoId} del carrito {UsuarioId}", productoId, usuarioId);

            var cart = await _cartRepository.GetCartByUserIdAsync(usuarioId);
            if (cart == null || !cart.Items.Any())
            {
                _logger.LogWarning("Intento de eliminar producto de un carrito inexistente. Usuario {UsuarioId}", usuarioId);
                throw new NotFoundException("CRT-001", "Carrito no encontrado.");
            }

            var itemExistente = cart.Items.FirstOrDefault(i => i.ProductoId == productoId);
            if (itemExistente == null)
            {
                _logger.LogWarning("El producto {ProductoId} no está en el carrito {UsuarioId}", productoId, usuarioId);
                throw new NotFoundException("CRT-002", "Producto no encontrado.");
            }

            cart.Items.Remove(itemExistente);
            cart.FechaActualizacion = DateTime.UtcNow;

            if (!cart.Items.Any())
            {
                // Si borró el único ítem, eliminamos la cabecera completa del carrito
                await _cartRepository.DeleteCartAsync(usuarioId);
                _logger.LogInformation("El carrito {UsuarioId} ha quedado vacío y fue eliminado de la base de datos.", usuarioId);
            }
            else
            {
                // Si aún quedan otros ítems, actualizamos
                await _cartRepository.CreateOrUpdateCartAsync(cart);
                _logger.LogInformation("Producto {ProductoId} eliminado. El carrito {UsuarioId} se actualizó.", productoId, usuarioId);
            }
        }

        // ── 5. VACIAR CARRITO COMPLETO (DELETE) ──
        public async Task ClearCartAsync(Guid usuarioId)
        {
            _logger.LogInformation("Vaciando carrito completo para el usuario {UsuarioId}", usuarioId);

            var cart = await _cartRepository.GetCartByUserIdAsync(usuarioId);
            if (cart == null)
            {
                _logger.LogWarning("Intento de vaciar carrito inexistente para el usuario {UsuarioId}", usuarioId);
                throw new NotFoundException("CRT-001", "Carrito no encontrado.");
            }

            await _cartRepository.DeleteCartAsync(usuarioId);
            _logger.LogInformation("Carrito {UsuarioId} vaciado y eliminado exitosamente.", usuarioId);
        }

        // ── MÉTODOS PRIVADOS AUXILIARES ──

        private async Task<ProductExternalResponse> ValidateProductStockAsync(Guid productoId, int requestedQuantity, int currentCartQuantity)
        {
            var client = _httpClientFactory.CreateClient();
            var response = await client.GetAsync($"https://localhost:7001/api/products/{productoId}");

            // VALIDACIÓN CRT-002: Producto inexistente en Products.API
            if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
            {
                _logger.LogWarning("Validación fallida: El producto {ProductoId} no existe en Products.API", productoId);
                throw new NotFoundException("CRT-002", "Producto no encontrado.");
            }

            response.EnsureSuccessStatusCode();
            var productoExterno = await response.Content.ReadFromJsonAsync<ProductExternalResponse>();

            int totalRequested = requestedQuantity + currentCartQuantity;

            // VALIDACIÓN CRT-003: Stock insuficiente
            if (productoExterno == null || productoExterno.Stock < totalRequested)
            {
                _logger.LogWarning("Validación fallida: Stock insuficiente. Producto {ProductoId}. Solicitado: {Total}, Disponible: {Stock}",
                    productoId, totalRequested, productoExterno?.Stock);

                throw new BusinessRuleException("CRT-003",
                    $"Stock insuficiente. Disponible: {productoExterno?.Stock ?? 0}, solicitado: {totalRequested}.");
            }

            return productoExterno;
        }

        private CartResponse MapToResponse(CartAPI.Models.Cart cart)
        {
            var itemsResponse = cart.Items.Select(i =>
                new CartItemResponse(i.ProductoId, i.Cantidad)
            ).ToList();

            return new CartResponse(
                cart.UsuarioId,
                itemsResponse,
                cart.FechaActualizacion
            );
        }
    }
}