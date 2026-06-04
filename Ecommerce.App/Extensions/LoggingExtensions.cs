using Serilog;
using Serilog.Events;
using Serilog.Filters;
using Microsoft.Extensions.Hosting;

namespace Ecommerce.App.Extensions;

public static class LoggingExtensions
{
    public static void AddAppLogging(this WebApplicationBuilder builder)
    {
        Log.Logger = new LoggerConfiguration()
            .MinimumLevel.Information()
            .MinimumLevel.Override("Microsoft", LogEventLevel.Warning)
            .Enrich.FromLogContext()
            .Enrich.WithProperty("Service", "Products.API")
            .Enrich.WithProperty("Environment", builder.Environment.EnvironmentName)

            // Console legible
            .WriteTo.Console(
            outputTemplate: "[{Timestamp:HH:mm:ss} {Level:u3}] {Message:lj}{NewLine}{Exception}"
            )

            // Archivo JSON estructurado
            .WriteTo.File(
                path: "logs/products-log-.json",
                rollingInterval: RollingInterval.Day,
                outputTemplate: "{Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz} [{Level:u3}] {Message:lj} {Properties:j}{NewLine}{Exception}")

            .CreateLogger();

        builder.Host.UseSerilog();
    }
}