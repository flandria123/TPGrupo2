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
                status = statusCode, // (O 500 directamente)
                detail = "Ocurrió un error inesperado en el servidor.",
                instance = context.Request.Path.Value,
                errorCode = "NTF-004", // <-- EL CÓDIGO EXACTO DE NOTIFICATIONS
                errorMessage = "Error interno al procesar la notificación.",

                // CorrelationId para trazabilidad distribuida
                correlationId = context.Items["CorrelationId"]?.ToString()
            }, cancellationToken);

            return true;
        }
    }
}
