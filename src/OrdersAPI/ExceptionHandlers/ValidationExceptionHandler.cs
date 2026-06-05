using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using OrdersAPI.Exceptions;

namespace OrdersAPI.ExceptionHandlers
{
    public class ValidationExceptionHandler : IExceptionHandler
    {
        private readonly ILogger<ValidationExceptionHandler> _logger;

        public ValidationExceptionHandler(ILogger<ValidationExceptionHandler> logger)
        {
            _logger = logger;
        }

        public async ValueTask<bool> TryHandleAsync(
            HttpContext context, Exception exception, CancellationToken cancellationToken)
        {
            // Si no es una excepción de validación, dejamos que pase al siguiente handler
            if (exception is not ValidationException ex) return false;

            // --- REQUERIMIENTO 5.3: LOGGING ---
            // Se registra como Warning porque el cliente envió datos mal armados
            _logger.LogWarning("Error de validación de datos. ErrorCode: {ErrorCode}. Mensaje: {ErrorMessage}", ex.ErrorCode, ex.Message);

            // --- REQUERIMIENTO 5.2: RESPUESTA ESTANDARIZADA ---
            context.Response.StatusCode = StatusCodes.Status400BadRequest;

            await context.Response.WriteAsJsonAsync(new
            {
                type = "https://tools.ietf.org/html/rfc7231#section-6.5.1",
                title = "Bad Request",
                status = 400,
                detail = "La solicitud contiene campos faltantes o con formato incorrecto.",
                instance = context.Request.Path.Value,
                errorCode = ex.ErrorCode,
                errorMessage = ex.Message
            }, cancellationToken);

            return true;
        }
    }
}
