namespace NotificationsAPI.Extensions;

using NotificationsAPI.Middleware;
using Serilog.Events;
using Serilog;

public static class MiddlewareExtensions
{
    public static void UseAppMiddleware(this WebApplication app)
    {
        // 1. Correlation ID Middleware
        // Genera o reutiliza el X-Correlation-Id
        // para trazabilidad distribuida.
        app.UseMiddleware<CorrelationIdMiddleware>();

        // 2. Audit Middleware
        // Registra operaciones de escritura
        // incluyendo request/response bodies sanitizados.
        app.UseMiddleware<AuditMiddleware>();

        // 3. Serilog Request Logging
        // Logging estructurado de requests HTTP.
        app.UseSerilogRequestLogging(options =>
        {
            options.GetLevel = (httpContext, _, ex) =>
            {
                // Error inesperado (5xx o excepción no controlada)
                if (ex != null || httpContext.Response.StatusCode >= 500)
                    return LogEventLevel.Error;

                // Errores de negocio / validación
                if (httpContext.Response.StatusCode >= 400)
                    return LogEventLevel.Warning;

                // Health checks silenciosos
                if (httpContext.Request.Path.StartsWithSegments("/health"))
                    return LogEventLevel.Verbose;

                // Requests exitosas
                return LogEventLevel.Information;
            };
        });
    }
}