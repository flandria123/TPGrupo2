using Microsoft.Extensions.Primitives;
using Serilog.Context;

namespace ProductsAPI.Middleware
{
    public class CorrelationIdMiddleware
    {
        private readonly RequestDelegate _next;
        private const string CorrelationIdHeader = "X-Correlation-Id";

        public CorrelationIdMiddleware(RequestDelegate next) => _next = next;

        public async Task InvokeAsync(HttpContext context)
        {
            // 1. Intentar obtener el ID (vital cuando Orders.API llama a Products.API)
            if (!context.Request.Headers.TryGetValue(CorrelationIdHeader, out StringValues correlationId) || string.IsNullOrWhiteSpace(correlationId))
            {
                // 2. Si no existe, generar uno nuevo
                correlationId = Guid.NewGuid().ToString();

                // Inyectarlo también en la Request para que otros middlewares posteriores lo lean del Header
                context.Request.Headers[CorrelationIdHeader] = correlationId;
            }

            // Guardarlo en el diccionario interno para acceso rápido (ideal para tu AuditMiddleware)
            context.Items["CorrelationId"] = correlationId.ToString();

            // 3. Suscribirse al evento OnStarting para inyectar el Header en la respuesta de forma segura
            context.Response.OnStarting(() =>
            {
                if (!context.Response.Headers.ContainsKey(CorrelationIdHeader))
                {
                    context.Response.Headers.Append(CorrelationIdHeader, correlationId);
                }
                return Task.CompletedTask;
            });

            // 4. "Empujarlo" al contexto de Serilog para que todos los logs de este request lo incluyan
            using (LogContext.PushProperty("CorrelationId", correlationId.ToString()))
            {
                // Pasar el control al siguiente middleware (probablemente el AuditMiddleware)
                await _next(context);
            }
        }
    }
}