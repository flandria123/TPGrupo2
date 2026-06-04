using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc; 

namespace ProductsAPI.ExceptionHandlers
{
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
            
            _logger.LogError(exception, "Ocurrió un error no manejado en Products.API: {Mensaje}", exception.Message);

            context.Response.StatusCode = StatusCodes.Status500InternalServerError;

           
            var correlationId = context.Request.Headers["X-Correlation-Id"].FirstOrDefault()
                                ?? context.TraceIdentifier;

            var problemDetails = new
            {
                type = "https://tools.ietf.org/html/rfc7231#section-6.6.1",
                title = "Internal Server Error",
                status = StatusCodes.Status500InternalServerError,
                detail = "Ocurrió un error interno en el servidor.",
                instance = context.Request.Path.Value,
                errorCode = "PRD-005", 
                errorMessage = "Error interno al procesar el producto.", 
                correlationId = correlationId 
            };

            await context.Response.WriteAsJsonAsync(problemDetails, cancellationToken);

            return true;
        }
    }
}