using Microsoft.AspNetCore.Diagnostics;
using Users.API.Exceptions;
namespace Users.API.ExceptionHandlers

{
    public class NotFoundExceptionHandler: IExceptionHandler
    {
        public async ValueTask<bool> TryHandleAsync(
        HttpContext context, Exception exception, CancellationToken cancellationToken)
        {
            // Solo maneja excepciones de tipo NotFoundException
            if (exception is not NotFoundException ex) return false;

            context.Response.StatusCode = StatusCodes.Status404NotFound;

            await context.Response.WriteAsJsonAsync(new
            {
                // URI obligatoria para 404 según RFC 7231
                type = "https://tools.ietf.org/html/rfc7231#section-6.5.4",
                title = "Not Found",
                status = 404,
                detail = "El recurso solicitado no fue encontrado.",
                instance = context.Request.Path.Value,
                errorCode = ex.ErrorCode,
                errorMessage = ex.Message
            }, cancellationToken);

            return true;
        }

    }
}
