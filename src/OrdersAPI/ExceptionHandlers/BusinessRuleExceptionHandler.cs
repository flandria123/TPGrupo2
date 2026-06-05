using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using OrdersAPI.Exceptions;

namespace OrdersAPI.ExceptionHandlers
{
    public class BusinessRuleExceptionHandler : IExceptionHandler
    {
        private readonly ILogger<BusinessRuleExceptionHandler> _logger;

        // Mantenemos la inyección del logger para cumplir con el req. 5.3 [cite: 13]
        public BusinessRuleExceptionHandler(ILogger<BusinessRuleExceptionHandler> logger)
        {
            _logger = logger;
        }

        public async ValueTask<bool> TryHandleAsync(
        HttpContext context, Exception exception, CancellationToken cancellationToken)
        {
            // Si la excepción no es de negocio, pasa al siguiente handler [cite: 9]
            if (exception is not BusinessRuleException ex) return false;

            // --- REQUERIMIENTO 5.3: LOGGING ---
           // Se registra como Warning e incluye el errorCode [cite: 15, 16]
            _logger.LogWarning("Violación de regla de negocio. ErrorCode: {ErrorCode}. Mensaje: {ErrorMessage}", ex.ErrorCode, ex.Message);

            // Mapeo dinámico de Status Code según el Catálogo de la Orders API
            var statusCode = ex.ErrorCode switch
            {
                "ORD-002" => StatusCodes.Status400BadRequest, // Datos inválidos
                "ORD-003" => StatusCodes.Status400BadRequest, // Usuario inexistente
                "ORD-004" => StatusCodes.Status400BadRequest, // Producto inexistente
                "ORD-005" => StatusCodes.Status400BadRequest, // Stock insuficiente
                "ORD-006" => StatusCodes.Status400BadRequest, // Transición de estado inválida
                _ => StatusCodes.Status400BadRequest          // Default por seguridad
            };

            context.Response.StatusCode = statusCode;

            // Construcción de la respuesta bajo estándar RFC 7231 
            await context.Response.WriteAsJsonAsync(new
            {
                type = GetTypeUri(statusCode),
                title = "Business Rule Violation",
                status = statusCode,
                detail = "Se ha producido un error de validación en la regla de negocio.",
                instance = context.Request.Path.Value,
               errorCode = ex.ErrorCode, // 
                errorMessage = ex.Message // 
            }, cancellationToken);

            return true;
        }

        private static string GetTypeUri(int statusCode) => statusCode switch
        {
            400 => "https://tools.ietf.org/html/rfc7231#section-6.5.1",
            422 => "https://tools.ietf.org/html/rfc4918#section-11.2", // Por si en la defensa te piden usar 422 para reglas de negocio
            _ => "https://tools.ietf.org/html/rfc7231#section-6.5.1"
        };
    }
}