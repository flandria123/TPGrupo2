namespace Users.API.Extensions;

using Serilog;
using Serilog.Core;
using Serilog.Events;
using Serilog.Filters;
using Serilog.Formatting.Json; 

public static class LoggingExtensions
{
    public static void AddAppLogging(this WebApplicationBuilder builder)
    {
        Log.Logger = new LoggerConfiguration()
            .MinimumLevel.Information()
            .MinimumLevel.Override("Microsoft", LogEventLevel.Warning)
            .MinimumLevel.Override("Microsoft.AspNetCore.Hosting.Diagnostics", LogEventLevel.Information)
            .Enrich.FromLogContext()

            // CONSOLA: Formato legible, solo errores para mantener la terminal limpia [3]
            .WriteTo.Logger(lc => lc
                .Filter.ByIncludingOnly(le => le.Level >= LogEventLevel.Error)
                .WriteTo.Console(outputTemplate: "[{Timestamp:HH:mm:ss} {Level:u3}] {Message:lj}{NewLine}{Exception}"))

            // ARCHIVO: Formato JSON estructurado para auditoría [2, 3]
            .WriteTo.Logger(lc => lc
                .Filter.ByIncludingOnly(le =>
                {
                    var sourceContext = le.Properties.ContainsKey("SourceContext")
        ? le.Properties["SourceContext"].ToString()
        : string.Empty;

                    // Permitir logs HTTP automáticos de Serilog
                    var isRequestLogging =
                        sourceContext.Contains("Serilog.AspNetCore.RequestLoggingMiddleware");

                    // Permitir logs de auditoría custom
                    var isAuditMiddleware =
                        sourceContext.Contains("AuditMiddleware");

                    if (!isRequestLogging && !isAuditMiddleware)
                        return false;

                    // Excluir endpoints irrelevantes
                    if (le.Properties.TryGetValue("RequestPath", out var pathValue) &&
                        pathValue is ScalarValue scalar &&
                        scalar.Value is string path)
                    {
                        return !path.Contains("/health", StringComparison.OrdinalIgnoreCase) &&
                               !path.Contains("/swagger", StringComparison.OrdinalIgnoreCase);
                    }

                    return true;
                })
                
                .WriteTo.File(
                    path: "logs/Users.API-audit.log",
                    formatter: new JsonFormatter(), // FORMATO OBLIGATORIO PARA EL TP [2]
                    rollingInterval: RollingInterval.Day))
            .CreateLogger();

        builder.Host.UseSerilog();
    }
}