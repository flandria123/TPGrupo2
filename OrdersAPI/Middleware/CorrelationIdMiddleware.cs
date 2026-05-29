using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Primitives;
using Serilog.Context;

namespace Orders.API.Middleware
{
    public class CorrelationIdMiddleware
    {
        private readonly RequestDelegate _next;
        private const string CorrelationIdHeaderName = "X-Correlation-Id";

        public CorrelationIdMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            // 1. Obtener el ID de la petición (si viene de otro microservicio) o crear uno nuevo
            if (!context.Request.Headers.TryGetValue(CorrelationIdHeaderName, out StringValues correlationId))
            {
                correlationId = Guid.NewGuid().ToString();
                context.Request.Headers.TryAdd(CorrelationIdHeaderName, correlationId);
            }

            // 2. Envolver la ejecución para que TODO lo que loguee Serilog incluya este ID
            using (LogContext.PushProperty("CorrelationId", correlationId.ToString()))
            {
                // 3. Asegurar que la respuesta al cliente también incluya este ID en sus cabeceras
                context.Response.OnStarting(() =>
                {
                    if (!context.Response.Headers.ContainsKey(CorrelationIdHeaderName))
                    {
                        context.Response.Headers.Append(CorrelationIdHeaderName, correlationId);
                    }
                    return Task.CompletedTask;
                });

                await _next(context);
            }
        }
    }
}