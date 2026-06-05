using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using OrdersAPI.Exceptions;

namespace OrdersAPI.ExceptionHandlers;

public class NotFoundExceptionHandler : IExceptionHandler
{
    private readonly ILogger<NotFoundExceptionHandler> _logger;

    // Inyectamos el logger por constructor
    public NotFoundExceptionHandler(ILogger<NotFoundExceptionHandler> logger)
    {
        _logger = logger;
    }

    public async ValueTask<bool> TryHandleAsync(HttpContext context, Exception exception, CancellationToken cancellationToken)
    {
        // Si no es una NotFoundException, pasamos al siguiente handler
        if (exception is not NotFoundException ex) return false;

        // --- REQUERIMIENTO 5.3: LOGGING ---
        // Se registra como Warning porque es un error de negocio esperado (ej. buscar un ID que no existe) [cite: 16]
        _logger.LogWarning("Recurso no encontrado. ErrorCode: {ErrorCode}. Mensaje: {ErrorMessage}", ex.ErrorCode, ex.Message);

        // --- REQUERIMIENTO 5.2: RESPUESTA ESTANDARIZADA ---
        context.Response.StatusCode = StatusCodes.Status404NotFound;

       // Se construye la respuesta con errorCode y errorMessage del catálogo 
        await context.Response.WriteAsJsonAsync(new
        {
            type = "https://tools.ietf.org/html/rfc7231#section-6.5.4",
            title = "Not Found",
            status = 404,
            detail = "El recurso solicitado no fue encontrado.",
            instance = context.Request.Path.Value,
            errorCode = ex.ErrorCode, // 
            errorMessage = ex.Message // 
        }, cancellationToken);

        return true;
    }
}