using CartAPI.Exceptions;
using Microsoft.AspNetCore.Diagnostics;

namespace CartAPI.ExceptionHandlers
{
    public class BusinessRuleExceptionHandler : IExceptionHandler
    {
        public async ValueTask<bool> TryHandleAsync(
            HttpContext context,
            Exception exception,
            CancellationToken cancellationToken)
        {
            
            if (exception is not BusinessRuleException ex)
            {
                return false; 
            }

            
            var statusCode = StatusCodes.Status422UnprocessableEntity;
            context.Response.StatusCode = statusCode;

           
            var correlationId = context.Request.Headers["X-Correlation-Id"].FirstOrDefault()
                                ?? context.TraceIdentifier;

            
            await context.Response.WriteAsJsonAsync(new
            {
                type = "https://tools.ietf.org/html/rfc4918#section-11.2",
                title = "Unprocessable Entity",
                status = statusCode,
                detail = "No se puede procesar la solicitud.",
                instance = context.Request.Path.Value,
                errorCode = ex.ErrorCode,
                errorMessage = ex.Message,
                correlationId = correlationId
            }, cancellationToken: cancellationToken);

            return true; 
        }
    }
}