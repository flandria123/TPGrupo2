using Microsoft.Extensions.Logging;
using OrdersAPI.Data;
using OrdersAPI.DTOs;
using OrdersAPI.Exceptions;
using OrdersAPI.Models;
using System.Net.Http.Json;

namespace OrdersAPI.Services;

public class OrderService : IOrderService
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly ILogger<OrderService> _logger;
    private readonly OrderRepository _repository; // Llamamos a la clase concreta directamente

    // Inyectamos el repositorio en el constructor
    public OrderService(IHttpClientFactory httpClientFactory, ILogger<OrderService> logger, OrderRepository repository)
    {
        _httpClientFactory = httpClientFactory;
        _logger = logger;
        _repository = repository;
    }

    public async Task<IEnumerable<OrderResponse>> GetOrdersAsync(Guid? usuarioId)
    {
        // Consumo directo desde la base de datos SQLite
        var orders = await _repository.GetAllAsync(usuarioId);

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

        // Validar que ninguna cantidad sea menor o igual a cero
        if (request.Items.Any(item => item.Cantidad <= 0))
        {
            throw new ValidationException("ORD-002", "Los datos de la orden son inválidos. La cantidad de los productos debe ser mayor a cero.");
        }

        // ====================================================================
        // 1. LLAMADA A USERS API (Validar usuario - ORD-003)
        // ====================================================================
        var usersClient = _httpClientFactory.CreateClient("UsersAPI");
        var userResponse = await usersClient.GetAsync($"/api/users/{request.UsuarioId}");

        if (!userResponse.IsSuccessStatusCode)
        {
            throw new NotFoundException("ORD-003", "El usuario especificado no existe o no es válido.");
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
                throw new NotFoundException("ORD-004", $"El producto con ID {item.ProductoId} no existe en el catálogo.");
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
            Items = orderItems
        };

        // Guardado real en la base de datos habilitado
        await _repository.AddAsync(nuevaOrden);

        _logger.LogInformation("Orden creada exitosamente. ID: {OrderId}, Total: ${Total}", nuevaOrden.Id, totalCalculado);

        return MapToResponse(nuevaOrden);
    }

    public async Task UpdateStatusAsync(Guid id, string nuevoEstado)
    {
        var order = await GetOrderEntityByIdAsync(id);

        // --- VALIDACIÓN DE REGLA DE NEGOCIO (ORD-006) - HABILITADA ---
        if (order.Estado == "Cancelada" || order.Estado == "Completada")
        {
            throw new BusinessRuleException("ORD-006", "No se puede modificar el estado de una orden que ya se encuentra en estado Cancelada o Completada.");
        }

        order.Estado = nuevoEstado;

        // Actualización real en la base de datos habilitada
        await _repository.UpdateAsync(order);
    }

    // ========================================================================
    // MÉTODOS PRIVADOS DE APOYO (Helpers)
    // ========================================================================

    private async Task<Order> GetOrderEntityByIdAsync(Guid id)
    {
        // Búsqueda real en base de datos habilitada
        Order? order = await _repository.GetByIdAsync(id);

        if (order == null)
        {
            throw new NotFoundException("ORD-001", "La orden con el ID especificado no existe.");
        }

        return order;
    }

    private OrderResponse MapToResponse(Order order)
    {
        return new OrderResponse
        {
            Id = order.Id,
            UsuarioId = order.UsuarioId,
            Total = order.Total,
            Estado = order.Estado,
            FechaCreacion = order.FechaCreacion,
            Items = order.Items?.Select(i => new OrderItemResponse
            {
                ProductoId = i.ProductoId,
                Cantidad = i.Cantidad,
                PrecioUnitario = i.PrecioUnitario
            }).ToList() ?? new List<OrderItemResponse>()
        };
    }
}