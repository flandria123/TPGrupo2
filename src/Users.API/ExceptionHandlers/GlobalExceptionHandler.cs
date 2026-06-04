using Microsoft.AspNetCore.Diagnostics;
using System.Net;
namespace Users.API.ExceptionHandlers
{
    public class GlobalExceptionHandler: IExceptionHandler
    {
        private readonly ILogger<GlobalExceptionHandler> _logger;

        public GlobalExceptionHandler(ILogger<GlobalExceptionHandler> logger)
        {
            _logger = logger;
        }

        public async ValueTask<bool> TryHandleAsync(
            HttpContext context, Exception exception, CancellationToken cancellationToken)
        {
            // 1. Loggear el error inesperado (Nivel Error según sección 5.3) [3]
            _logger.LogError(exception, "Ocurrió un error no controlado: {Message}", exception.Message);

            // 2. Configurar el status code 500 [2]
            context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;

            // 3. Construir la respuesta bajo estándar RFC 7231 [4]
            await context.Response.WriteAsJsonAsync(new
            {
                type = "https://tools.ietf.org/html/rfc7231#section-6.6.1", // URI para Error 500
                title = "Internal Server Error",
                status = 500,
                detail = "Ocurrió un error inesperado en el servidor.",
                instance = context.Request.Path.Value,
                errorCode = "USR-006", // Código obligatorio del catálogo [2]
                errorMessage = "Error interno al procesar el usuario. Error inesperado en servicio o persistencia." // Mensaje sugerido [2]
            }, cancellationToken);

            // Retornamos true porque este es el último eslabón de la cadena [1]
            return true;
        }


    }
}
