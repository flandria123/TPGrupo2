using Microsoft.AspNetCore.Diagnostics;
using NotificationsAPI.Exceptions;

namespace NotificationsAPI.ExceptionHandlers
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

            const int statusCode = StatusCodes.Status400BadRequest;

            context.Response.StatusCode = statusCode;

            await context.Response.WriteAsJsonAsync(new
            {
                type = "https://tools.ietf.org/html/rfc9110#section-15.5.1",
                title = "Validation Error",
                status = statusCode,
                detail = "La solicitud contiene datos inválidos.",
                instance = context.Request.Path.Value,

                
                errorCode = ex.ErrorCode,
                errorMessage = ex.Message,

                
                correlationId = context.Items["CorrelationId"]?.ToString()
            }, cancellationToken);

            return true;
        }
    }
}
