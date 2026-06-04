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

        public CartService(
            CartRepository cartRepository,
            IHttpClientFactory httpClientFactory,
            ILogger<CartService> logger)
        {
            _cartRepository = cartRepository;
            _httpClientFactory = httpClientFactory;
            _logger = logger;
        }

        // ──────────────────────────────────────────────────────────────────────────
        // OBTENER CARRITO
        // ──────────────────────────────────────────────────────────────────────────
        public async Task<CartResponse> GetCartAsync(Guid usuarioId)
        {
            var cart = await _cartRepository.GetCartByUserIdAsync(usuarioId);

            if (cart == null)
            {
                _logger.LogWarning(
                    "Carrito inexistente para usuario {UsuarioId}. [CRT-001]",
                    usuarioId);

                throw new NotFoundException(
                    "CRT-001",
                    "Carrito no encontrado.");
            }

            _logger.LogInformation(
                "Carrito obtenido correctamente para usuario {UsuarioId}",
                usuarioId);

            return MapToResponse(cart);
        }

        // ──────────────────────────────────────────────────────────────────────────
        // AGREGAR PRODUCTO AL CARRITO
        // ──────────────────────────────────────────────────────────────────────────
        public async Task<CartResponse> AddItemAsync(
            Guid usuarioId,
            AddItemRequest request)
        {
            // Validación CRT-004
            if (request.Cantidad <= 0)
            {
                _logger.LogWarning(
                    "Cantidad inválida para producto {ProductoId}. [CRT-004]",
                    request.ProductoId);

                throw new ValidationException(
                    "CRT-004",
                    "Cantidad inválida.");
            }

            // Validar producto vía ProductsAPI
            var productsClient =
                _httpClientFactory.CreateClient("ProductsAPI");

            var response = await productsClient.GetAsync(
                $"/api/products/{request.ProductoId}");

            // CRT-002 → Producto inexistente
            if (!response.IsSuccessStatusCode)
            {
                _logger.LogWarning(
                    "Producto inexistente {ProductoId}. [CRT-002]",
                    request.ProductoId);

                throw new NotFoundException(
                    "CRT-002",
                    "Producto no encontrado.");
            }

            var product = await response.Content
                .ReadFromJsonAsync<ProductExternalResponse>();

            // CRT-003 → Stock insuficiente
            if (product == null || product.Stock < request.Cantidad)
            {
                _logger.LogWarning(
                    "Stock insuficiente para producto {ProductoId}. [CRT-003]",
                    request.ProductoId);

                throw new BusinessRuleException(
                    "CRT-003",
                    $"Stock insuficiente. Disponible: {product?.Stock ?? 0}, solicitado: {request.Cantidad}.");
            }

            // Buscar carrito existente
            var cart = await _cartRepository.GetCartByUserIdAsync(usuarioId);

            // Si no existe, crear uno nuevo
            if (cart == null)
            {
                cart = new CartAPI.Models.Cart
                {
                    UsuarioId = usuarioId,
                    FechaActualizacion = DateTime.UtcNow,
                    Items = new List<CartItem>()
                };

                await _cartRepository.CreateOrUpdateCartAsync(cart);
            }

            // Buscar producto existente en carrito
            var existingItem = cart.Items
                .FirstOrDefault(i => i.ProductoId == request.ProductoId);

            if (existingItem != null)
            {
                existingItem.Cantidad += request.Cantidad;
            }
            else
            {
                cart.Items.Add(new CartItem
                {
                    ProductoId = request.ProductoId,
                    Cantidad = request.Cantidad
                });
            }

            cart.FechaActualizacion = DateTime.UtcNow;

            await _cartRepository.CreateOrUpdateCartAsync(cart);

            _logger.LogInformation(
                "Producto {ProductoId} agregado al carrito del usuario {UsuarioId}",
                request.ProductoId,
                usuarioId);

            return MapToResponse(cart);
        }

        // ──────────────────────────────────────────────────────────────────────────
        // ACTUALIZAR CANTIDAD
        // ──────────────────────────────────────────────────────────────────────────
        public async Task<CartResponse> UpdateItemAsync(
            Guid usuarioId,
            Guid productoId,
            UpdateItemRequest request)
        {
            // CRT-004
            if (request.Cantidad <= 0)
            {
                _logger.LogWarning(
                    "Cantidad inválida para producto {ProductoId}. [CRT-004]",
                    productoId);

                throw new ValidationException(
                    "CRT-004",
                    "Cantidad inválida.");
            }

            var cart = await _cartRepository.GetCartByUserIdAsync(usuarioId);

            // CRT-001
            if (cart == null)
            {
                _logger.LogWarning(
                    "Carrito inexistente para usuario {UsuarioId}. [CRT-001]",
                    usuarioId);

                throw new NotFoundException(
                    "CRT-001",
                    "Carrito no encontrado.");
            }

            var item = cart.Items
                .FirstOrDefault(i => i.ProductoId == productoId);

            // CRT-002
            if (item == null)
            {
                _logger.LogWarning(
                    "Producto inexistente en carrito {ProductoId}. [CRT-002]",
                    productoId);

                throw new NotFoundException(
                    "CRT-002",
                    "Producto no encontrado.");
            }

            // Validar stock nuevamente
            var productsClient =
                _httpClientFactory.CreateClient("ProductsAPI");

            var response = await productsClient.GetAsync(
                $"/api/products/{productoId}");

            if (!response.IsSuccessStatusCode)
            {
                _logger.LogWarning(
                    "Producto inexistente {ProductoId}. [CRT-002]",
                    productoId);

                throw new NotFoundException(
                    "CRT-002",
                    "Producto no encontrado.");
            }

            var product = await response.Content
                .ReadFromJsonAsync<ProductExternalResponse>();

            // CRT-003
            if (product == null || product.Stock < request.Cantidad)
            {
                _logger.LogWarning(
                    "Stock insuficiente para producto {ProductoId}. [CRT-003]",
                    productoId);

                throw new BusinessRuleException(
                    "CRT-003",
                    $"Stock insuficiente. Disponible: {product?.Stock ?? 0}, solicitado: {request.Cantidad}.");
            }

            item.Cantidad = request.Cantidad;

            cart.FechaActualizacion = DateTime.UtcNow;

            await _cartRepository.CreateOrUpdateCartAsync(cart);

            _logger.LogInformation(
                "Cantidad actualizada para producto {ProductoId} del usuario {UsuarioId}",
                productoId,
                usuarioId);

            return MapToResponse(cart);
        }

        // ──────────────────────────────────────────────────────────────────────────
        // ELIMINAR PRODUCTO DEL CARRITO
        // ──────────────────────────────────────────────────────────────────────────
        public async Task RemoveItemAsync(
            Guid usuarioId,
            Guid productoId)
        {
            var cart = await _cartRepository.GetCartByUserIdAsync(usuarioId);

            // CRT-001
            if (cart == null)
            {
                _logger.LogWarning(
                    "Carrito inexistente para usuario {UsuarioId}. [CRT-001]",
                    usuarioId);

                throw new NotFoundException(
                    "CRT-001",
                    "Carrito no encontrado.");
            }

            var item = cart.Items
                .FirstOrDefault(i => i.ProductoId == productoId);

            // CRT-002
            if (item == null)
            {
                _logger.LogWarning(
                    "Producto inexistente en carrito {ProductoId}. [CRT-002]",
                    productoId);

                throw new NotFoundException(
                    "CRT-002",
                    "Producto no encontrado.");
            }

            cart.Items.Remove(item);

            cart.FechaActualizacion = DateTime.UtcNow;

            await _cartRepository.CreateOrUpdateCartAsync(cart);

            _logger.LogInformation(
                "Producto {ProductoId} eliminado del carrito del usuario {UsuarioId}",
                productoId,
                usuarioId);
        }

        // ──────────────────────────────────────────────────────────────────────────
        // VACIAR CARRITO
        // ──────────────────────────────────────────────────────────────────────────
        public async Task ClearCartAsync(Guid usuarioId)
        {
            var cart = await _cartRepository.GetCartByUserIdAsync(usuarioId);

            // CRT-001
            if (cart == null)
            {
                _logger.LogWarning(
                    "Carrito inexistente para usuario {UsuarioId}. [CRT-001]",
                    usuarioId);

                throw new NotFoundException(
                    "CRT-001",
                    "Carrito no encontrado.");
            }

            cart.Items.Clear();

            cart.FechaActualizacion = DateTime.UtcNow;

            await _cartRepository.CreateOrUpdateCartAsync(cart);

            _logger.LogInformation(
                "Carrito vaciado para usuario {UsuarioId}",
                usuarioId);
        }

        // ──────────────────────────────────────────────────────────────────────────
        // MAPPING
        // ──────────────────────────────────────────────────────────────────────────
        private CartResponse MapToResponse(CartAPI.Models.Cart cart)
        {
            return new CartResponse
            {
                UsuarioId = cart.UsuarioId,
                FechaActualizacion = cart.FechaActualizacion,

                Items = cart.Items.Select(i => new CartItemResponse
                {
                    ProductoId = i.ProductoId,
                    Cantidad = i.Cantidad
                }).ToList()
            };
        }
    }



}