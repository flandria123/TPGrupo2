using Microsoft.AspNetCore.Builder;
using Serilog;
using Serilog.Events;
using Users.API.Middleware;

// se configura el middleware que intercepta cada request HTTP y genera el evento de log:

namespace Users.API.Extensions
{
    public static class MiddlewareExtensions
    {

        public static void UseAppMiddleware(this WebApplication app)
        {
            // 1. Correlation ID Middleware
            // Genera o reutiliza el X-Correlation-Id para trazabilidad distribuida.
            app.UseMiddleware<CorrelationIdMiddleware>();

            // 2. Audit Middleware
            // Registra operaciones de escritura (POST, PUT, DELETE)
            // incluyendo request/response bodies sanitizados.
            app.UseMiddleware<AuditMiddleware>();

            // 3. Serilog Request Logging
            // Logging estructurado de requests HTTP.
            app.UseSerilogRequestLogging(options =>
            {
                options.GetLevel = (httpContext, _, ex) =>
                    ex != null
                        ? LogEventLevel.Error
                        : httpContext.Request.Path.StartsWithSegments("/health")
                            ? LogEventLevel.Verbose
                            : LogEventLevel.Information;
            });
        }




    }
}
