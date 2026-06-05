using Serilog;
using Serilog.Events;
using Serilog.Filters;
using Serilog.Formatting.Json;

namespace Orders.API.Extensions
{
    public static class LoggingExtensions
    {
        public static void AddAppLogging(this WebApplicationBuilder builder)
        {
            Log.Logger = new LoggerConfiguration()
                .MinimumLevel.Information()
                .MinimumLevel.Override("Microsoft", LogEventLevel.Warning)
                .MinimumLevel.Override("Microsoft.AspNetCore.Hosting.Diagnostics", LogEventLevel.Information)
                .Enrich.FromLogContext() // Atrapa el CorrelationId de nuestro middleware

                // LOG DE CONSOLA: Mantenemos errores, texto plano
                .WriteTo.Logger(lc => lc
                    .Filter.ByIncludingOnly(le => le.Level >= LogEventLevel.Error)
                    .WriteTo.Console(outputTemplate: "[{Timestamp:HH:mm:ss} {Level:u3}] {CorrelationId} {Message:lj}{NewLine}{Exception}"))

                // LOG DE ARCHIVO: Auditoría limpia en formato JSON
                .WriteTo.Logger(lc => lc
                    .Filter.ByIncludingOnly(le =>
                    {
                        var isSerilogMiddleware = Matching.FromSource("Serilog.AspNetCore.RequestLoggingMiddleware")(le);
                        if (!isSerilogMiddleware) return false;

                        if (le.Properties.TryGetValue("RequestPath", out var pathValue) &&
                            pathValue is ScalarValue scalar && scalar.Value is string path)
                        {
                            return !path.Contains("/health", StringComparison.OrdinalIgnoreCase) &&
                                   !path.Contains("/swagger", StringComparison.OrdinalIgnoreCase);
                        }

                        return true;
                    })
                    .WriteTo.File(
                        formatter: new JsonFormatter(), // Formato JSON estructurado
                        path: "logs/orders-audit.json", // Extension en .json
                        rollingInterval: RollingInterval.Day))
                .CreateLogger();

            builder.Host.UseSerilog();
        }
    }
}
