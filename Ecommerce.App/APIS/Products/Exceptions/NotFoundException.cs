namespace Ecommerce.App.Products.API.Exceptions;

/// <summary>
/// Excepcion que se lanza cuando no se encuentra un recurso (404).
/// </summary>
public class NotFoundException : Exception
{
    /// <summary>
    /// Codigo de error segun el catalogo del TP (ej: PRD-001)
    /// </summary>
    public string ErrorCode { get; }

    public NotFoundException(string errorCode, string message)
        : base(message)
    {
        ErrorCode = errorCode;
    }
}