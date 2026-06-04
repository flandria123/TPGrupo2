using Microsoft.AspNetCore.Diagnostics;
namespace Ecommerce.App.Products.API.ExceptionHandlers;

/// <summary>
/// Handler global para cualquier excepción no controlada (Error 500)
/// </summary>
public class GlobalExceptionHandler : IExceptionHandler
{
    private readonly ILogger<GlobalExceptionHandler> _logger;

    public GlobalExceptionHandler(ILogger<GlobalExceptionHandler> logger)
    {
        _logger = logger;
    }

    public async ValueTask<bool> TryHandleAsync(
        HttpContext context,
        Exception exception,
        CancellationToken cancellationToken)
    {
        _logger.LogError(exception, "Ocurrió un error inesperado");

        context.Response.StatusCode = StatusCodes.Status500InternalServerError;

        var problemDetails = new
        {
            type = "https://tools.ietf.org/html/rfc7231#section-6.6.1",
            title = "Internal Server Error",
            status = 500,
            detail = "Ocurrió un error interno en el servidor.",
            instance = context.Request.Path.Value,
            errorCode = "PRD-005",
            errorMessage = "Error interno al procesar el producto."
        };

        await context.Response.WriteAsJsonAsync(problemDetails, cancellationToken);
        return true;
    }
}