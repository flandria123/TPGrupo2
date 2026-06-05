using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc; 
using ProductsAPI.Exceptions;

namespace ProductsAPI.ExceptionHandlers
{
    public class ValidationExceptionHandler : IExceptionHandler
    {
        public async ValueTask<bool> TryHandleAsync(
            HttpContext context,
            Exception exception,
            CancellationToken cancellationToken)
        {
            if (exception is not ValidationException ex)
                return false;

            context.Response.StatusCode = StatusCodes.Status400BadRequest;

            
            var correlationId = context.Request.Headers["X-Correlation-Id"].FirstOrDefault()
                                ?? context.TraceIdentifier;

            var problemDetails = new
            {
                type = "https://tools.ietf.org/html/rfc7231#section-6.5.1",
                title = "Bad Request",
                status = StatusCodes.Status400BadRequest, 
                detail = "Los datos del producto son inválidos.", 
                instance = context.Request.Path.Value,
                errorCode = ex.ErrorCode,
                errorMessage = ex.Message,
                correlationId = correlationId 
            };

            await context.Response.WriteAsJsonAsync(problemDetails, cancellationToken);
            return true;
        }
    }
}