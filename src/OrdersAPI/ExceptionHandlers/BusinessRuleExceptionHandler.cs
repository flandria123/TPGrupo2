using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using OrdersAPI.Exceptions;

namespace OrdersAPI.ExceptionHandlers
{
    using Microsoft.AspNetCore.Diagnostics;
    using Microsoft.AspNetCore.Http;
    using Microsoft.Extensions.Logging;

    public class BusinessRuleExceptionHandler : IExceptionHandler
    {
        private readonly ILogger<BusinessRuleExceptionHandler> _logger;

        // Mantenemos la inyección del logger para cumplir con el req. 5.3 
        public BusinessRuleExceptionHandler(ILogger<BusinessRuleExceptionHandler> logger)
        {
            _logger = logger;
        }

        public async ValueTask<bool> TryHandleAsync(
        HttpContext context, Exception exception, CancellationToken cancellationToken)
        {
            // Si la excepción no es de negocio, pasa al siguiente handler [cite: 46]
            if (exception is not BusinessRuleException ex) return false;

            // --- REQUERIMIENTO 5.3: LOGGING ---
            // Se registra como Warning e incluye el errorCode [cite: 34]
            _logger.LogWarning("Violación de regla de negocio. ErrorCode: {ErrorCode}. Mensaje: {ErrorMessage}", ex.ErrorCode, ex.Message);

            // Mapeo dinámico de Status Code según el Catálogo de la Orders API 
            var statusCode = ex.ErrorCode switch
            {
                "ORD-005" => StatusCodes.Status422UnprocessableEntity, // Stock insuficiente
                "ORD-006" => StatusCodes.Status409Conflict,            // Transición de estado inválida
                _ => StatusCodes.Status400BadRequest                   // Default por seguridad
            };

            context.Response.StatusCode = statusCode;

            // Títulos exactos según los ejemplos del TP 
            var title = statusCode switch
            {
                422 => "Unprocessable Entity",
                409 => "Conflict",
                _ => "Business Rule Violation"
            };

            // Detalles exactos según los ejemplos del TP 
            var detail = statusCode switch
            {
                422 => "No se puede procesar la solicitud.",
                409 => "No se puede modificar el estado.",
                _ => "Se ha producido un error de validación en la regla de negocio."
            };

            // Construcción de la respuesta bajo estándar RFC [cite: 22, 23]
            await context.Response.WriteAsJsonAsync(new
            {
                type = GetTypeUri(statusCode),
                title = title,
                status = statusCode,
                detail = detail,
                instance = context.Request.Path.Value,
                errorCode = ex.ErrorCode,
                errorMessage = ex.Message
            }, cancellationToken);

            return true;
        }

        private static string GetTypeUri(int statusCode) => statusCode switch
        {
            422 => "https://tools.ietf.org/html/rfc4918#section-11.2", // Para ORD-005 [cite: 22]
            409 => "https://tools.ietf.org/html/rfc7231#section-6.5.9", // Para ORD-006 [cite: 23]
            _ => "https://tools.ietf.org/html/rfc7231#section-6.5.1"
        };
    }
}