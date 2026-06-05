using Microsoft.Extensions.Primitives;
using Serilog.Context;

namespace CartAPI.Middleware
{
    public class CorrelationIdMiddleware
    {
        private readonly RequestDelegate _next;
        private const string CorrelationIdHeader = "X-Correlation-Id";

        public CorrelationIdMiddleware(RequestDelegate next) => _next = next;

        public async Task InvokeAsync(HttpContext context)
        {
            // 1. Intentar obtener el ID del header (por si la request viene del API Gateway u otro servicio)
            if (!context.Request.Headers.TryGetValue(CorrelationIdHeader, out StringValues correlationId) || string.IsNullOrWhiteSpace(correlationId))
            {
                // 2. Si no existe, generar uno nuevo
                correlationId = Guid.NewGuid().ToString();
            }

            // Asignarlo a la propiedad nativa de .NET para el seguimiento de la request
            context.TraceIdentifier = correlationId.ToString();

            // 3. Guardarlo también en Items para que el AuditMiddleware lo pueda leer fácilmente
            context.Items["CorrelationId"] = context.TraceIdentifier;

            // 4. Suscribirse al evento OnStarting para inyectar el Header en la respuesta de forma segura
            context.Response.OnStarting(() =>
            {
                if (!context.Response.Headers.ContainsKey(CorrelationIdHeader))
                {
                    context.Response.Headers.Append(CorrelationIdHeader, context.TraceIdentifier);
                }
                return Task.CompletedTask;
            });

            // 5. Empujarlo al contexto de Serilog para que aparezca en todos los logs estructurados
            using (LogContext.PushProperty("CorrelationId", context.TraceIdentifier))
            {
                await _next(context);
            }
        }
    }
}