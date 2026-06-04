using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc; 

namespace CartAPI.ExceptionHandlers
{
    
    public class GlobalExceptionHandler(ILogger<GlobalExceptionHandler> logger) : IExceptionHandler
    {
        public async ValueTask<bool> TryHandleAsync(
            HttpContext context,
            Exception exception,
            CancellationToken cancellationToken)
        {
            
            logger.LogError(exception, "Ocurrió un error no manejado en Cart.API: {Mensaje}", exception.Message);

            
            var statusCode = StatusCodes.Status500InternalServerError;
            context.Response.StatusCode = statusCode;

            
            var correlationId = context.Request.Headers["X-Correlation-Id"].FirstOrDefault()
                                ?? context.TraceIdentifier;

            
            await context.Response.WriteAsJsonAsync(new
            {
                type = "https://tools.ietf.org/html/rfc7231#section-6.6.1",
                title = "Internal Server Error",
                status = statusCode,
                detail = "Ocurrió un error inesperado en el servidor.",
                instance = context.Request.Path.Value,
                errorCode = "CRT-005", 
                errorMessage = "Error interno al procesar el carrito.", 
                correlationId = correlationId
            }, cancellationToken: cancellationToken);

           
            return true;
        }
    }
}