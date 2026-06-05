namespace ProductsAPI.DTOs
{
    public record ProductResponse(

    /// <example>3fa85f64-5717-4562-b3fc-2c963f66afa6</example>
    Guid Id,

    /// <example>Notebook Dell XPS 15</example>
    string Nombre,

    /// <example>Laptop 15 pulgadas, 32GB RAM</example>
    string? Descripcion,

    /// <example>1500.00</example>
    decimal Precio,

    /// <example>10</example>
    int Stock,

    /// <example>Electrónica</example>
    string Categoria,

    /// <example>2024-01-15T10:30:00Z</example>
    DateTime FechaCreacion

    );
}
