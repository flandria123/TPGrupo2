using Microsoft.AspNetCore.Diagnostics;
using ProductsAPI.Exceptions;

namespace ProductsAPI.ExceptionHandlers
{
    public class ValidationExceptionHandler: IExceptionHandler
    {
        public async ValueTask<bool> TryHandleAsync(
        HttpContext context,
        Exception exception,
        CancellationToken cancellationToken)
        {
            if (exception is not ValidationException ex)
                return false;

            context.Response.StatusCode = StatusCodes.Status400BadRequest;

            var problemDetails = new
            {
                type = "https://tools.ietf.org/html/rfc7231#section-6.5.1", 
                title = "Bad Request",
                status = 400,
                detail = "Los datos proporcionados no son válidos.",
                instance = context.Request.Path.Value,
                errorCode = ex.ErrorCode,     
                errorMessage = ex.Message,    

                
                correlationId = context.TraceIdentifier
            };

            await context.Response.WriteAsJsonAsync(problemDetails, cancellationToken);
            return true;
        }



    }
}
