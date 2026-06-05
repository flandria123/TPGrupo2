using Microsoft.AspNetCore.Diagnostics;
using NotificationsAPI.Exceptions;

namespace NotificationsAPI.ExceptionHandlers
{
    public class NotFoundExceptionHandler : IExceptionHandler
    {
        public async ValueTask<bool> TryHandleAsync(
            HttpContext context,
            Exception exception,
            CancellationToken cancellationToken)
        {
            if (exception is not NotFoundException ex)
                return false;

            const int statusCode = StatusCodes.Status404NotFound;

            context.Response.StatusCode = statusCode;

            await context.Response.WriteAsJsonAsync(new
            {
                type = "https://datatracker.ietf.org/doc/html/rfc9110#section-15.5.5",
                title = "Not Found",
                status = statusCode,
                detail = "El recurso solicitado no fue encontrado.",
                instance = context.Request.Path.Value,

               
                errorCode = ex.ErrorCode,
                errorMessage = ex.Message,

                
                correlationId = context.Items["CorrelationId"]?.ToString()
            }, cancellationToken);

            return true;
        }
    }
}
