using Ecommerce.App.Products.API.Exceptions;
using Microsoft.AspNetCore.Diagnostics;
using System.Text.Json;

namespace Ecommerce.App.Products.API.ExceptionHandlers;

/// <summary>
/// Maneja las excepciones de tipo NotFoundException (Error 404)
/// </summary>
public class NotFoundExceptionHandler : IExceptionHandler
{
    public async ValueTask<bool> TryHandleAsync(
        HttpContext context,
        Exception exception,
        CancellationToken cancellationToken)
    {
        if (exception is not NotFoundException ex)
            return false;

        context.Response.StatusCode = StatusCodes.Status404NotFound;

        var problemDetails = new
        {
            type = "https://tools.ietf.org/html/rfc7231#section-6.5.4",
            title = "Not Found",
            status = 404,
            detail = "El recurso solicitado no fue encontrado.",
            instance = context.Request.Path.Value,
            errorCode = ex.ErrorCode,
            errorMessage = ex.Message
        };

        await context.Response.WriteAsJsonAsync(problemDetails, cancellationToken);
        return true;
    }
}