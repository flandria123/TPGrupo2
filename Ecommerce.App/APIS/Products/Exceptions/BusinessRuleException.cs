namespace Ecommerce.App.Products.API.Exceptions;

/// <summary>
/// Excepcion que se lanza cuando se viola una regla de negocio (409 Conflict).
/// </summary>
public class BusinessRuleException : Exception
{
    /// <summary>
    /// Codigo de error según el catálogo (ej: PRD-003, PRD-004)
    /// </summary>
    public string ErrorCode { get; }

    public BusinessRuleException(string errorCode, string message)
        : base(message)
    {
        ErrorCode = errorCode;
    }
}