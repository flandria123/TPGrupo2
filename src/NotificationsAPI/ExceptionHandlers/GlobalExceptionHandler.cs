using Microsoft.AspNetCore.Diagnostics;

namespace NotificationsAPI.ExceptionHandlers
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
            // Log interno del error inesperado
            _logger.LogError(
                exception,
                "Error no controlado en Notifications.API. Path: {Path}",
                context.Request.Path.Value);

            const int statusCode = StatusCodes.Status500InternalServerError;

            context.Response.StatusCode = statusCode;

            await context.Response.WriteAsJsonAsync(new
            {
                type = "https://tools.ietf.org/html/rfc7231#section-6.6.1",
                title = "Internal Server Error",
                status = statusCode,
                detail = "Ocurrió un error interno inesperado.",
                instance = context.Request.Path.Value,

                // CorrelationId para trazabilidad distribuida
                correlationId = context.Items["CorrelationId"]?.ToString()
            }, cancellationToken);

            return true;
        }
    }
}
