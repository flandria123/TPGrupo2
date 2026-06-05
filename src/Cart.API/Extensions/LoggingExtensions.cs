using Serilog;
using Serilog.Events;
using Serilog.Filters;

namespace CartAPI.Extensions;

public static class LoggingExtensions
{
    public static void AddAppLogging(this WebApplicationBuilder builder)
    {
        Log.Logger = new LoggerConfiguration()
            .MinimumLevel.Information()
            .MinimumLevel.Override("Microsoft", LogEventLevel.Warning)
            .MinimumLevel.Override("Microsoft.AspNetCore.Hosting.Diagnostics", LogEventLevel.Information)
            .Enrich.FromLogContext()
            // Etiquetamos todos los logs con el nombre de este microservicio
            .Enrich.WithProperty("Service", "Cart.API")
            .Enrich.WithProperty("Environment", builder.Environment.EnvironmentName)

            // ── CONSOLA: Solo muestra errores críticos para no saturar la terminal ──
            .WriteTo.Logger(lc => lc
                .Filter.ByIncludingOnly(le => le.Level >= LogEventLevel.Error)
                .WriteTo.Console(outputTemplate: "[{Timestamp:HH:mm:ss} {Level:u3}] {Message:lj}{NewLine}{Exception}"))

            // ── ARCHIVO: Registro de Auditoría (Audit.log) ──
            .WriteTo.Logger(lc => lc
                .Filter.ByIncludingOnly(le =>
                {
                    // Capturamos el request logger de Serilog Y nuestro custom AuditMiddleware
                    var isSerilogMiddleware = Matching.FromSource("Serilog.AspNetCore.RequestLoggingMiddleware")(le);
                    var isCustomAudit = Matching.FromSource("Cart.API.Middleware.AuditMiddleware")(le);

                    if (!isSerilogMiddleware && !isCustomAudit) return false;

                    // Ignorar endpoints de HealthCheck y Swagger para no generar "ruido" en el archivo de texto
                    if (le.Properties.TryGetValue("RequestPath", out var p) && p is ScalarValue s && s.Value is string path)
                        return !path.Contains("/health") && !path.Contains("/swagger");

                    return true;
                })
                .WriteTo.File(
                    path: "logs/audit.log",
                    // Se agrega {Message:lj} para que se imprima la cadena completa del AuditMiddleware
                    outputTemplate: "{Timestamp:yyyy-MM-dd HH:mm:ss} | {Level:u3} | {Message:lj}{NewLine}{Exception}",
                    rollingInterval: RollingInterval.Day))
            .CreateLogger();

        builder.Host.UseSerilog();
    }
}