using Microsoft.AspNetCore.Mvc;
using OrdersAPI.DTOs;
using OrdersAPI.Models;

namespace OrdersAPI.Controllers;

[ApiController]
[Route("api/orders")] // La ruta base que pide la sección 4.3: /api/orders
public class OrdersController : ControllerBase
{
    // 1. GET /api/orders (Listar órdenes, opcional por usuario)
    [HttpGet]
    public IActionResult GetOrders([FromQuery] Guid? usuarioId)
    {
        // Aquí irá la lógica para listar
        return Ok(new List<Order>());
    }

    // 2. GET /api/orders/{id} (Obtener detalle de una orden)
    [HttpGet("{id}")]
    public IActionResult GetOrderById(Guid id)
    {
        // Aquí irá la lógica para buscar por ID
        return Ok(new Order());
    }

    // 3. POST /api/orders (Crear una nueva orden)
    [HttpPost]
    public IActionResult CreateOrder([FromBody] CreateOrderRequest request)
    {
        // Aquí irá la lógica de calcular total, validar stock y guardar
        return CreatedAtAction(nameof(GetOrderById), new { id = Guid.NewGuid() }, request);
    }

    // 4. PUT /api/orders/{id}/status (Actualizar estado de la orden)
    [HttpPut("{id}/status")]
    public IActionResult UpdateStatus(Guid id, [FromBody] string nuevoEstado)
    {
        // Aquí irá la lógica de cambiar estado (Pendiente, Confirmada, etc.)
        return NoContent();
    }
}
