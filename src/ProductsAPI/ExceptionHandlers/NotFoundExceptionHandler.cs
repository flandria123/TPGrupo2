using Microsoft.AspNetCore.Diagnostics;
using ProductsAPI.Exceptions;
using System.Text.Json;

namespace ProductsAPI.ExceptionHandlers;


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
            errorMessage = ex.Message,


             correlationId = context.TraceIdentifier
        };


        await context.Response.WriteAsJsonAsync(problemDetails, cancellationToken);
        return true;
    }
}