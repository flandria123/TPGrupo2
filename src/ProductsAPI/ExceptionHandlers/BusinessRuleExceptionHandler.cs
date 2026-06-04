using ProductsAPI.Exceptions;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc; // Para StatusCodes

namespace ProductsAPI.ExceptionHandlers
{
    public class BusinessRuleExceptionHandler : IExceptionHandler
    {
        public async ValueTask<bool> TryHandleAsync(
            HttpContext context,
            Exception exception,
            CancellationToken cancellationToken)
        {
            if (exception is not BusinessRuleException ex)
                return false;

            
            context.Response.StatusCode = StatusCodes.Status409Conflict;

            
            var correlationId = context.Request.Headers["X-Correlation-Id"].FirstOrDefault()
                                ?? context.TraceIdentifier;

            var problemDetails = new
            {
                type = "https://tools.ietf.org/html/rfc7231#section-6.5.9",
                title = "Conflict",
                status = StatusCodes.Status409Conflict,
                detail = "No se puede procesar la solicitud por un conflicto de reglas de negocio.",
                instance = context.Request.Path.Value,
                errorCode = ex.ErrorCode,
                errorMessage = ex.Message,
                correlationId = correlationId // Rastreo inter-servicios [cite: 34]
            };

            await context.Response.WriteAsJsonAsync(problemDetails, cancellationToken);
            return true;
        }
    }
}