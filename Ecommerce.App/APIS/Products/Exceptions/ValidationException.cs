namespace Ecommerce.App.Products.API.Exceptions;

/// <summary>
/// Excepción para errores de validación de datos (400 Bad Request).
/// </summary>
public class ValidationException : Exception
{
    public string ErrorCode { get; }

    public ValidationException(string errorCode, string message)
        : base(message)
    {
        ErrorCode = errorCode;
    }
}