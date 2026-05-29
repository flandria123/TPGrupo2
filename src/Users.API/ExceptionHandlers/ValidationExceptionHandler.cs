using Microsoft.AspNetCore.Diagnostics;
using Users.API.Exceptions;

namespace Users.API.ExceptionHandlers
{
    public class ValidationExceptionHandler: IExceptionHandler

    {
        public async ValueTask<bool> TryHandleAsync(
         HttpContext context, Exception exception, CancellationToken cancellationToken)
        {
            if (exception is not ValidationException ex) return false;

            context.Response.StatusCode = StatusCodes.Status400BadRequest; // 400 para USR-002 [6]

            await context.Response.WriteAsJsonAsync(new
            {
                type = "https://tools.ietf.org/html/rfc7231#section-6.5.1",
                title = "Bad Request",
                status = 400,
                detail = "Los datos proporcionados son inválidos.",
                instance = context.Request.Path.Value,
                errorCode = ex.ErrorCode, // Aquí irá "USR-002" [6]
                errorMessage = ex.Message
            }, cancellationToken);

            return true;
        }



    }
}
