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
            
           // app.UseMiddleware<CorrelationIdMiddleware>();

            // 2. Audit Middleware
            
            //app.UseMiddleware<AuditMiddleware>();

            // 3. Serilog Request Logging
           
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

                    // Requests normales exitosas
                    return LogEventLevel.Information;
                };
            });
        }




    }
}