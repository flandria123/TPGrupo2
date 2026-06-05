using Microsoft.AspNetCore.Diagnostics;
using Users.API.Exceptions;


namespace Users.API.ExceptionHandlers

{
    public class BusinessRuleExceptionHandler: IExceptionHandler
    {
        public async ValueTask<bool> TryHandleAsync(
        HttpContext context, Exception exception, CancellationToken cancellationToken)
        {
            if (exception is not BusinessRuleException ex) return false;

            // Mapeo dinámico de Status Code según el Catálogo de la Users API [1, 2]
            var statusCode = ex.ErrorCode switch
            {
                "USR-001" => StatusCodes.Status409Conflict,      // Email duplicado
                "USR-003" => StatusCodes.Status401Unauthorized,  // Credenciales incorrectas
                "USR-004" => StatusCodes.Status403Forbidden,     // Bloqueo por intentos
                "USR-005" => StatusCodes.Status403Forbidden,     // Bloqueo por fraude
                _ => StatusCodes.Status400BadRequest             // Default por seguridad
            };

            context.Response.StatusCode = statusCode;

            // Construcción de la respuesta bajo estándar RFC 7231 [4, 5]
            await context.Response.WriteAsJsonAsync(new
            {
                type = GetTypeUri(statusCode),
                title = "Business Rule Violation",
                status = statusCode,
                detail = "Se ha violado una regla de negocio del sistema.",
                instance = context.Request.Path.Value,
                errorCode = ex.ErrorCode,
                errorMessage = ex.Message
            }, cancellationToken);

            return true;
        }

        private static string GetTypeUri(int statusCode) => statusCode switch
        {
            401 => "https://tools.ietf.org/html/rfc7235#section-3.1",
            409 => "https://tools.ietf.org/html/rfc7231#section-6.5.9",
            403 => "https://tools.ietf.org/html/rfc7231#section-6.5.3", // USR-004 y USR-005: Forbidden [3, 4]
            _ => "https://tools.ietf.org/html/rfc7231#section-6.5.3"
        };



    }
}
