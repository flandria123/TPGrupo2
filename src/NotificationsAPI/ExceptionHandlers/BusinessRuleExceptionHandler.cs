using Microsoft.AspNetCore.Diagnostics;
using NotificationsAPI.Exceptions;

namespace NotificationsAPI.ExceptionHandlers
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

            const int statusCode = StatusCodes.Status409Conflict;

            context.Response.StatusCode = statusCode;

            await context.Response.WriteAsJsonAsync(new
            {
                type = "https://tools.ietf.org/html/rfc9110#section-15.5.10",
                title = "Conflicto de regla de negocio",
                status = statusCode,
                detail = "La operación no puede completarse debido a una regla de negocio.",
                instance = context.Request.Path.Value,
                errorCode = ex.ErrorCode,
                errorMessage = ex.Message
            }, cancellationToken);

            return true;
        }
    }
}
