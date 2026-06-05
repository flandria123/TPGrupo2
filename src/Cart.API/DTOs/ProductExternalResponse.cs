namespace CartAPI.DTOs
{

    /// <summary>
    /// DTO de uso interno para deserializar las respuestas HTTP de la Products.API.
    /// No se expone en los endpoints de Swagger de Cart.API.
    /// </summary>
    public record ProductExternalResponse(
        Guid Id,
        string Nombre,
        string Descripcion,
        decimal Precio,
        int Stock,
        string Categoria,
        DateTime FechaCreacion
    );


}