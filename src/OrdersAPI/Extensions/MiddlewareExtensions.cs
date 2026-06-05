using HealthChecks.UI.Client;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Orders.API.Middleware;
using Serilog;
using Serilog.Events;

namespace Orders.API.Extensions
{
    public static class MiddlewareExtensions
    {
        public static void UseAppMiddleware(this WebApplication app)
        {
            // 1. Manejo global de excepciones
            app.UseExceptionHandler();

            // 2. Swagger (Solo en desarrollo)
            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI();
            }

            // 3. Correlation ID (Debe ir ANTES de Serilog para que el ID se loguee)
            app.UseMiddleware<CorrelationIdMiddleware>();

            // 4. Log de auditoría automático de Serilog
            app.UseSerilogRequestLogging(options =>
            {
                options.GetLevel = (httpContext, _, ex) =>
                    (ex != null) ? Serilog.Events.LogEventLevel.Error :
                        (httpContext.Request.Path.StartsWithSegments("/health"))
                            ? Serilog.Events.LogEventLevel.Verbose : Serilog.Events.LogEventLevel.Information;
            });

            // 5. Nuestro middleware para auditar el JSON del Body
            app.UseMiddleware<AuditMiddleware>();

            // 6. Configuración de Health Checks 
            // A. Endpoint general detallado (JSON)
            app.MapHealthChecks("/health", new HealthCheckOptions
            {
                ResponseWriter = UIResponseWriter.WriteHealthCheckUIResponse
            });

            // B. Endpoint 'Live' (Filtra por el tag "live")
            app.MapHealthChecks("/health/live", new HealthCheckOptions
            {
                Predicate = check => check.Tags.Contains("live")
            });

            // C. Endpoint 'Ready' (Filtra por el tag "ready")
            app.MapHealthChecks("/health/ready", new HealthCheckOptions
            {
                Predicate = check => check.Tags.Contains("ready")
            });

            // D. Dashboard Web (UI)
            app.MapHealthChecksUI(setup => setup.UIPath = "/health-ui");
        }
    }
}
