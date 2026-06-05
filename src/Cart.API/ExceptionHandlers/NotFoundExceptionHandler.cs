using CartAPI.Exceptions;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc; 

namespace CartAPI.ExceptionHandlers
{
    public class NotFoundExceptionHandler : IExceptionHandler
    {
        public async ValueTask<bool> TryHandleAsync(
            HttpContext context,
            Exception exception,
            CancellationToken cancellationToken)
        {
            
            if (exception is not NotFoundException ex)
            {
                return false;
            }

            
            var statusCode = StatusCodes.Status404NotFound;
            context.Response.StatusCode = statusCode;

            
            var correlationId = context.Request.Headers["X-Correlation-Id"].FirstOrDefault()
                                ?? context.TraceIdentifier;

            
            await context.Response.WriteAsJsonAsync(new
            {
                type = "https://tools.ietf.org/html/rfc7231#section-6.5.4",
                title = "Not Found",
                status = statusCode,
                detail = "El recurso solicitado no fue encontrado.",
                instance = context.Request.Path.Value,
                errorCode = ex.ErrorCode,
                errorMessage = ex.Message,
                correlationId = correlationId 
            }, cancellationToken: cancellationToken);

            return true; 
        }
    }
}