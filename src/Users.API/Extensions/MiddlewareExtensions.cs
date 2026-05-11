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
            // Acá se puede agregar cualquier middleware personalizado que necesite

            // 1. CORRLEATION ID MIDDLEWARE: Genera o captura el X-Correlation-Id.
            // Es decir, asegura que el ID se genere o capture antes de cualquier otra acción

            app.UseMiddleware<CorrelationIdMiddleware>();

            // 2. AUDIT MIDDLEWARE
            app.UseMiddleware<AuditMiddleware>();

            // 3. SERILOG REQUEST LOGGING: Middleware oficial de Serilog para auditoría HTTP.
            // Al ejecutarse después, este log ya incluirá automáticamente el Correlation ID [1, 2].
            app.UseSerilogRequestLogging(options =>
            {
                options.GetLevel = (httpContext, _, ex) =>
               (ex != null) ? LogEventLevel.Error :
               (httpContext.Request.Path.StartsWithSegments("/health"))
               ? LogEventLevel.Verbose : LogEventLevel.Information;


            });


        }




    }
}
