using Microsoft.Extensions.Logging;
using OrdersAPI.DTOs;
using OrdersAPI.Exceptions;
using OrdersAPI.Models;
using System.Net.Http.Json;

namespace OrdersAPI.Services;

public class OrderService : IOrderService
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly ILogger<OrderService> _logger;

    public OrderService(IHttpClientFactory httpClientFactory, ILogger<OrderService> logger)
    {
        _httpClientFactory = httpClientFactory;
        _logger = logger;
    }

    public async Task<IEnumerable<OrderResponse>> GetOrdersAsync(Guid? usuarioId)
    {
        // Simulación: Acá iría var orders = await _repository.GetAllAsync(usuarioId);
        var orders = new List<Order>();

        // Transformamos la lista de entidades en una lista de DTOs
        return orders.Select(MapToResponse).ToList();
    }

    public async Task<OrderResponse> GetOrderByIdAsync(Guid id)
    {
        // Buscamos la entidad real usando el método privado
        var order = await GetOrderEntityByIdAsync(id);

        // Devolvemos el DTO
        return MapToResponse(order);
    }

    public async Task<OrderResponse> CreateOrderAsync(CreateOrderRequest request)
    {
        // --- VALIDACIÓN DE ENTRADA (ORD-002) ---
        if (request.Items == null || !request.Items.Any())
        {
            throw new ValidationException("ORD-002", "Los datos de la orden son inválidos. La lista de items está vacía.");
        }

        // ====================================================================
        // 1. LLAMADA A USERS API (Validar usuario - ORD-003)
        // ====================================================================
        var usersClient = _httpClientFactory.CreateClient("UsersAPI");
        var userResponse = await usersClient.GetAsync($"/api/users/{request.UsuarioId}");

        if (!userResponse.IsSuccessStatusCode)
        {
            throw new BusinessRuleException("ORD-003", "El usuario especificado no existe o no es válido.");
        }

        // ====================================================================
        // 2. LLAMADA A PRODUCTS API (Validar stock y precio - ORD-004, ORD-005)
        // ====================================================================
        var productsClient = _httpClientFactory.CreateClient("ProductsAPI");
        decimal totalCalculado = 0;
        var orderItems = new List<OrderItem>();

        foreach (var item in request.Items)
        {
            var productResponse = await productsClient.GetAsync($"/api/products/{item.ProductoId}");

            if (!productResponse.IsSuccessStatusCode)
            {
                throw new BusinessRuleException("ORD-004", $"El producto con ID {item.ProductoId} no existe en el catálogo.");
            }

            var productData = await productResponse.Content.ReadFromJsonAsync<ProductExternalResponse>();

            if (productData == null || productData.Stock < item.Cantidad)
            {
                throw new BusinessRuleException("ORD-005", $"Stock insuficiente para el producto {item.ProductoId}. Stock actual: {productData?.Stock ?? 0}.");
            }

            totalCalculado += productData.Precio * item.Cantidad;

            orderItems.Add(new OrderItem
            {
                ProductoId = item.ProductoId,
                Cantidad = item.Cantidad,
                PrecioUnitario = productData.Precio
            });
        }

        // ====================================================================
        // 3. PERSISTENCIA (Modelos reales habilitados)
        // ====================================================================
        var nuevaOrden = new Order
        {
            Id = Guid.NewGuid(),
            UsuarioId = request.UsuarioId,
            Total = totalCalculado,
            Estado = "Pendiente",
            FechaCreacion = DateTime.UtcNow,
            Items = orderItems // Asignamos los items al modelo principal
        };

        // await _repository.AddAsync(nuevaOrden);

        _logger.LogInformation("Orden creada exitosamente. ID: {OrderId}, Total: ${Total}", nuevaOrden.Id, totalCalculado);

        return MapToResponse(nuevaOrden);
    }

    public async Task UpdateStatusAsync(Guid id, string nuevoEstado)
    {
        // Usamos el helper interno que nos devuelve la ENTIDAD (Order) para poder mutarla
        var order = await GetOrderEntityByIdAsync(id);

        // --- VALIDACIÓN DE REGLA DE NEGOCIO (ORD-006) - HABILITADA ---
        if (order.Estado == "Cancelada" || order.Estado == "Completada")
        {
            throw new BusinessRuleException("ORD-006", "No se puede modificar el estado de una orden que ya se encuentra en estado Cancelada o Completada.");
        }

        // Asignamos el estado actualizado a nuestra entidad
        order.Estado = nuevoEstado;

        // await _repository.UpdateAsync(order);
    }

    // ========================================================================
    // MÉTODOS PRIVADOS DE APOYO (Helpers)
    // ========================================================================

    // Devuelve la Entidad pura para uso interno (Ej: para actualizar estado)
    private async Task<Order> GetOrderEntityByIdAsync(Guid id)
    {
        Order? order = null; // Acá iría: await _repository.GetByIdAsync(id);

        if (order == null)
        {
            throw new NotFoundException("ORD-001", "La orden con el ID especificado no existe.");
        }

        return order;
    }

    // Centraliza la conversión de la Base de Datos a la Respuesta de la API
    private OrderResponse MapToResponse(Order order)
    {
        return new OrderResponse
        {
            Id = order.Id,
            UsuarioId = order.UsuarioId,
            Total = order.Total,
            Estado = order.Estado,
            FechaCreacion = order.FechaCreacion,
            // Asumiendo que tu modelo Order tiene una colección 'Items'
            Items = order.Items?.Select(i => new OrderItemResponse
            {
                ProductoId = i.ProductoId,
                Cantidad = i.Cantidad,
                PrecioUnitario = i.PrecioUnitario
            }).ToList() ?? new List<OrderItemResponse>()
        };
    }
}