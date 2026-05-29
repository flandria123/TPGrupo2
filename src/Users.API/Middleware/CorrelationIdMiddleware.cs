using Microsoft.Extensions.Primitives;
using Serilog.Context;
using System.Diagnostics;
namespace Users.API.Middleware
{
    public class CorrelationIdMiddleware
    {
        private readonly RequestDelegate _next;
        private const string CorrelationIdHeader = "X-Correlation-Id";

        public CorrelationIdMiddleware(RequestDelegate next) => _next = next;

        public async Task InvokeAsync(HttpContext context)
        {
            // 1. Intentar obtener el ID del header (si viene de otro microservicio)
            if (!context.Request.Headers.TryGetValue(CorrelationIdHeader, out StringValues correlationId) || string.IsNullOrWhiteSpace(correlationId))
            {
                // 2. Si no existe, generar uno nuevo (GUID) [1]
                correlationId = Guid.NewGuid().ToString();
            }
            context.Items["CorrelationId"] = correlationId.ToString();

            // 3. Añadirlo a la respuesta para que el cliente lo vea [1]
            context.Response.Headers.Append(CorrelationIdHeader, correlationId);

            // 4. "Empujarlo" al contexto de Serilog para que todos los logs lo incluyan [2]
            using (LogContext.PushProperty("CorrelationId", correlationId.ToString()))
            {
                await _next(context);
            }
        }
    }

}  
