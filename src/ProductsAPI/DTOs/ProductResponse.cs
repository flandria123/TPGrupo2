namespace ProductsAPI.DTOs
{
    /// <summary>
    /// Representa los datos de un producto devuelto al cliente.
    /// </summary>
    public record ProductResponse
    {
        /// <summary>ID único del producto.</summary>
        /// <example>3fa85f64-5717-4562-b3fc-2c963f66afa6</example>
        public required Guid Id { get; init; }

        /// <summary>Nombre del producto.</summary>
        /// <example>Notebook Dell XPS 15</example>
        public required string Nombre { get; init; }

        /// <summary>Descripción detallada del producto.</summary>
        /// <example>Laptop 15 pulgadas, 32GB RAM</example>
        public string? Descripcion { get; init; }

        /// <summary>Precio del producto.</summary>
        /// <example>1500.00</example>
        public required decimal Precio { get; init; }

        /// <summary>Cantidad disponible en stock.</summary>
        /// <example>10</example>
        public required int Stock { get; init; }

        /// <summary>Categoría a la que pertenece el producto.</summary>
        /// <example>Electrónica</example>
        public required string Categoria { get; init; }

        /// <summary>Fecha en la que se creó el registro del producto.</summary>
        /// <example>2024-01-15T10:30:00Z</example>
        public required DateTime FechaCreacion { get; init; }
    }
}
