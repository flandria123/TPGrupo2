using CartAPI.Exceptions;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc; // Para StatusCodes

namespace CartAPI.ExceptionHandlers
{
    public class ValidationExceptionHandler : IExceptionHandler
    {
        public async ValueTask<bool> TryHandleAsync(
            HttpContext context,
            Exception exception,
            CancellationToken cancellationToken)
        {
            
            if (exception is not ValidationException ex)
            {
                return false; 
            }

           
            var statusCode = StatusCodes.Status400BadRequest;
            context.Response.StatusCode = statusCode;

            
            var correlationId = context.Request.Headers["X-Correlation-Id"].FirstOrDefault()
                                ?? context.TraceIdentifier;

           
            await context.Response.WriteAsJsonAsync(new
            {
                type = "https://tools.ietf.org/html/rfc7231#section-6.5.1",
                title = "Bad Request",
                status = statusCode,
                detail = "Los datos ingresados son inválidos.",
                instance = context.Request.Path.Value,
                errorCode = ex.ErrorCode,
                errorMessage = ex.Message,
                correlationId = correlationId 
            }, cancellationToken: cancellationToken);

            return true; 
        }
    }
}