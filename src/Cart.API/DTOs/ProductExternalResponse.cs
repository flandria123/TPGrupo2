namespace CartAPI.DTOs
{

    /// <summary>
    /// DTO de uso interno para deserializar las respuestas HTTP de la Products.API.
    /// No se expone en los endpoints de Swagger de Cart.API.
    /// </summary>
    public record ProductExternalResponse
    {
        public required Guid Id { get; init; }
        public required string Nombre { get; init; }
        public required string Descripcion { get; init; }
        public required decimal Precio { get; init; }
        public required int Stock { get; init; }
        public required string Categoria { get; init; }
        public required DateTime FechaCreacion { get; init; }
    }


}