using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace OrdersAPI.ExceptionHandlers
{
    public class GlobalExceptionHandler : IExceptionHandler
    {
        private readonly ILogger<GlobalExceptionHandler> _logger;

        public GlobalExceptionHandler(ILogger<GlobalExceptionHandler> logger)
        {
            _logger = logger;
        }

        public async ValueTask<bool> TryHandleAsync(
            HttpContext context, Exception exception, CancellationToken cancellationToken)
        {
            // --- REQUERIMIENTO 5.3: LOGGING ---
            // Los errores no esperados se loguean como Error, incluyendo el Stack Trace
            // en el archivo de log para que vos puedas debuggear, pero NO se le envía al cliente.
            _logger.LogError(exception, "Ha ocurrido un error interno no controlado en el sistema.");

            // --- REQUERIMIENTO 5.2: EVITAR EXPOSICIÓN DE STACK TRACE ---
            context.Response.StatusCode = StatusCodes.Status500InternalServerError;

            await context.Response.WriteAsJsonAsync(new
            {
                type = "https://tools.ietf.org/html/rfc7231#section-6.6.1",
                title = "Internal Server Error",
                status = 500,
                detail = "Se ha producido un error interno en el servidor. Contacte al administrador.",
                instance = context.Request.Path.Value,
                errorCode = "ORD-007", 
                errorMessage = "Error interno inesperado."
            }, cancellationToken);

            // Al retornar true, cortamos la ejecución y evitamos que .NET muestre su pantalla amarilla de error.
            return true;
        }
    }
}
