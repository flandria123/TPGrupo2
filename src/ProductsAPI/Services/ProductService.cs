using Microsoft.Extensions.Logging;
using ProductsAPI.DTOs;
using ProductsAPI.Models;
using ProductsAPI.Exceptions;
using ProductsAPI.Data;
using System.Net.Http.Json;

namespace ProductsAPI.Services;

public class ProductService : IProductService
{
    private readonly ProductRepository _repository;
    private readonly ILogger<ProductService> _logger;
    private readonly IHttpClientFactory _httpClientFactory;

    public ProductService(
        ProductRepository repository,
        ILogger<ProductService> logger,
        IHttpClientFactory httpClientFactory)
    {
        _repository = repository;
        _logger = logger;
        _httpClientFactory = httpClientFactory;
    }

    
    public async Task<IEnumerable<ProductResponse>> GetAllAsync(string? categoria, string? nombre)
    {
        var products = await _repository.GetAllAsync(categoria, nombre);

        return products.Select(p => new ProductResponse(
            p.Id, p.Nombre, p.Descripcion, p.Precio,
            p.Stock, p.Categoria, p.FechaCreacion
        ));
    }

    
    public async Task<ProductResponse> GetByIdAsync(Guid id)
    {
       _logger.LogInformation("Buscando producto con ID {Id}", id);
        var product = await _repository.GetByIdAsync(id);

        if (product == null)
        {
            _logger.LogWarning("Intento de obtener producto inexistente: {Id}", id);
            throw new NotFoundException("PRD-001", "Producto no encontrado.");
        }

        return new ProductResponse(
            product.Id, product.Nombre, product.Descripcion, product.Precio,
            product.Stock, product.Categoria, product.FechaCreacion
        );
    }


    public async Task<ProductResponse> CreateAsync(CreateProductRequest request)
    {
        // ── 1. VALIDACIÓN PRD-002: Falla rápido (Fail-Fast) antes de ir a la BD ──
        if (string.IsNullOrWhiteSpace(request.Nombre) || request.Precio <= 0 || request.Stock < 0)
        {
            _logger.LogWarning("Intento de crear producto con datos inválidos. Nombre: '{Nombre}', Precio: {Precio}, Stock: {Stock}",
                request.Nombre, request.Precio, request.Stock);

            throw new ValidationException("PRD-002", "Los datos del producto son inválidos.");
        }

        // ── 2. VALIDACIÓN PRD-003: Buscar duplicados ──
        var existe = await _repository.ExistsByNameAndCategoryAsync(request.Nombre, request.Categoria);
        if (existe)
        {
            _logger.LogWarning("Conflicto al crear: Ya existe el producto {Nombre} en la categoría {Categoria}", request.Nombre, request.Categoria);
            throw new BusinessRuleException("PRD-003", $"Ya existe un producto con ese nombre en la categoría '{request.Categoria}'.");
        }

        // ── 3. CREACIÓN DEL PRODUCTO (Con la fecha exigida) ──
        var newProduct = new Product
        {
            Id = Guid.NewGuid(),
            Nombre = request.Nombre,
            Descripcion = request.Descripcion,
            Precio = request.Precio,
            Stock = request.Stock,
            Categoria = request.Categoria,
            FechaCreacion = DateTime.UtcNow
        };

        await _repository.CreateAsync(newProduct);
        _logger.LogInformation("Producto creado con éxito: {Id}", newProduct.Id);

        // ── 4. MAPEO AL DTO DE RESPUESTA ──
        return new ProductResponse(
            newProduct.Id, newProduct.Nombre, newProduct.Descripcion, newProduct.Precio,
            newProduct.Stock, newProduct.Categoria, newProduct.FechaCreacion
        );
    }

    public async Task<ProductResponse> UpdateAsync(Guid id, UpdateProductRequest request)
    {
        // ── 1. VALIDACIÓN PRD-002: Fallar rápido si los datos son basur
        // Regla de negocio: Precio mayor a 0, Stock mayor o igual a 0, Nombre requerido 
        if (string.IsNullOrWhiteSpace(request.Nombre) || request.Precio <= 0 || request.Stock < 0)
        {
            _logger.LogWarning("Intento de actualizar producto con datos inválidos. Precio: {Precio}, Stock: {Stock}",
                request.Precio, request.Stock);

            throw new ValidationException("PRD-002", "Los datos del producto son inválidos.");
        }

        // ── 2. BUSCAR EL PRODUCTO ──
        var product = await _repository.GetByIdAsync(id);

        // ── 3. VALIDACIÓN PRD-001: Verificar que exista ──
        if (product == null)
        {
            _logger.LogWarning("Intento de actualizar producto inexistente: {Id}", id);
            throw new NotFoundException("PRD-001", "Producto no encontrado.");
        }

        // ── 4. ACTUALIZAR PROPIEDADES ──
        product.Nombre = request.Nombre;
        product.Descripcion = request.Descripcion;
        product.Precio = request.Precio;
        product.Stock = request.Stock;
        product.Categoria = request.Categoria;

        await _repository.UpdateAsync(product);
        _logger.LogInformation("Producto {Id} actualizado con éxito", id);

        return new ProductResponse(
            product.Id, product.Nombre, product.Descripcion, product.Precio,
            product.Stock, product.Categoria, product.FechaCreacion
        );
    }

    public async Task DeleteAsync(Guid id)
    {
        var product = await _repository.GetByIdAsync(id);

        if (product == null)
        {
            _logger.LogWarning("Intento de eliminar producto inexistente: {Id}", id);
            throw new NotFoundException("PRD-001", "Producto no encontrado.");
        }
        
        
        var ordersClient = _httpClientFactory.CreateClient("OrdersAPI");

        try
        {
            var response = await ordersClient.GetAsync($"/api/orders?productoId={id}");

            if (response.IsSuccessStatusCode)
            {
                var ordenes = await response.Content.ReadFromJsonAsync<IEnumerable<dynamic>>();

                if (ordenes != null && ordenes.Any(o => (string)o.estado == "Pendiente" || (string)o.estado == "Confirmada"))
                {
                    _logger.LogWarning("No se puede eliminar el producto {Id} porque tiene órdenes activas.", id);
                    throw new BusinessRuleException("PRD-004", "El producto tiene órdenes activas y no puede eliminarse.");
                }
            }
        }
        catch (Exception ex) when (ex is not BusinessRuleException)
        {
            _logger.LogError(ex, "Error al comunicarse con Orders.API para validar borrado del producto {Id}.", id);
            throw new Exception("Error de comunicación entre microservicios.", ex);
        }

        await _repository.DeleteAsync(id);
        _logger.LogInformation("Producto {Id} eliminado con éxito", id);
    }
}

