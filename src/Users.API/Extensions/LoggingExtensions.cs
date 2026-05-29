namespace Users.API.Extensions;

using Serilog;
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
                    // Solo logs del Middleware de Serilog para evitar duplicados [4]
                    var isSerilogMiddleware = Matching.FromSource("Serilog.AspNetCore.RequestLoggingMiddleware")(le);
                    if (!isSerilogMiddleware) return false;

                    // Exclusión de rutas de monitoreo y docs [3, 4]
                    if (le.Properties.TryGetValue("RequestPath", out var pathValue) &&
                        pathValue is ScalarValue scalar && scalar.Value is string path)
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