namespace Users.API.Extensions;

using Serilog;
using Serilog.Events;
using Serilog.Filters;
using Serilog.Core;
using Microsoft.Extensions.Hosting;

public static class LoggingExtensions
{
    public static void AddAppLogging(this WebApplicationBuilder builder)
    {
        Log.Logger = new LoggerConfiguration()
            .MinimumLevel.Information()  // Globalmente, solo registrar eventos de nivel Information o superior
            .MinimumLevel.Override("Microsoft", LogEventLevel.Warning) // Menos ruidoso para los logs de Microsoft, solo Warning o superior
            .MinimumLevel.Override("Microsoft.AspNetCore.Hosting.Diagnostics",
             LogEventLevel.Information)
            .Enrich.FromLogContext()

             .WriteTo.Logger(lc => lc
                    .Filter.ByIncludingOnly(le => le.Level >= LogEventLevel.Error)
                    .WriteTo.Console(outputTemplate: "[{Timestamp:HH:mm:ss} {Level:u3}] {Message:lj}{NewLine}{Exception}"))

             .WriteTo.Logger(lc => lc
                    .Filter.ByIncludingOnly(le =>
                    {
                        // Solo aceptar logs que vengan del Middleware de Serilog
                        // Esto elimina automáticamente los duplicados de Microsoft.AspNetCore.Mvc
                        var isSerilogMiddleware = Matching.FromSource("Serilog.AspNetCore.RequestLoggingMiddleware")(le);
                        if (!isSerilogMiddleware) return false;

                        // Excluir rutas irrelevantes
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
                        outputTemplate: "{Timestamp:yyyy-MM-dd HH:mm:ss} | {RequestMethod} | {RequestPath} | {StatusCode}{NewLine}",
                        rollingInterval: RollingInterval.Day))
                
             
                      .CreateLogger();




              builder.Host.UseSerilog();
    }
}

