
using CartAPI.Middleware;
using Microsoft.AspNetCore.Builder;
using Serilog;
using Serilog.Events;

namespace CartAPI.Extensions
{
    public static class MiddlewareExtensions
    {
        public static void UseAppMiddleware(this WebApplication app)
        {
            // 1. Correlation ID Middleware (Debe ir primero para generar el ID)
            app.UseMiddleware<CorrelationIdMiddleware>();

            // 2. Audit Middleware (Mide tiempos y captura el body usando el ID generado)
            app.UseMiddleware<AuditMiddleware>();

            // 3. Serilog Request Logging (Logueo nativo del framework)
            app.UseSerilogRequestLogging(options =>
            {
                options.GetLevel = (httpContext, _, ex) =>
                {
                    // Error inesperado (5xx o excepción no controlada) -> Error
                    if (ex != null || httpContext.Response.StatusCode >= 500)
                        return LogEventLevel.Error;

                    // Errores de negocio / validación (4xx) -> Warning
                    if (httpContext.Response.StatusCode >= 400)
                        return LogEventLevel.Warning;

                    // Health checks silenciosos para no inundar los logs
                    if (httpContext.Request.Path.StartsWithSegments("/health"))
                        return LogEventLevel.Verbose;

                    // Requests normales exitosas -> Information
                    return LogEventLevel.Information;
                };
            });
        }
    }
}