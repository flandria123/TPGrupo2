using Serilog;
using Serilog.Events;
using Serilog.Filters;
using Microsoft.Extensions.Hosting;

namespace ProductsAPI.Extensions;

public static class LoggingExtensions
{
    public static void AddAppLogging(this WebApplicationBuilder builder)
    {
        Log.Logger = new LoggerConfiguration()
            .MinimumLevel.Information() 
            .MinimumLevel.Override("Microsoft", LogEventLevel.Warning)
            .MinimumLevel.Override("Microsoft.AspNetCore.Hosting.Diagnostics", LogEventLevel.Information)
            .Enrich.FromLogContext()
            .Enrich.WithProperty("Service", "Products.API")
            .Enrich.WithProperty("Environment", builder.Environment.EnvironmentName)

            
            .WriteTo.Logger(lc => lc
                .Filter.ByIncludingOnly(le => le.Level >= LogEventLevel.Error)
                .WriteTo.Console(outputTemplate: "[{Timestamp:HH:mm:ss} {Level:u3}] {Message:lj}{NewLine}{Exception}"))

           
            .WriteTo.Logger(lc => lc
                .Filter.ByIncludingOnly(le =>
                {
                    var esSerilogMiddleware = Matching.FromSource("Serilog.AspNetCore.RequestLoggingMiddleware")(le);
                    if (!esSerilogMiddleware) return false;

                    if (le.Properties.TryGetValue("RequestPath", out var p) && p is ScalarValue s && s.Value is string path)
                        return !path.Contains("/health") && !path.Contains("/swagger"); 

                    return true;
                })
                .WriteTo.File(
                    path: "logs/audit.log",
                    outputTemplate: "{Timestamp:yyyy-MM-dd HH:mm:ss} | {RequestMethod} | {RequestPath} | {StatusCode}{NewLine}",
                    rollingInterval: RollingInterval.Day)) 
            .CreateLogger();

        builder.Host.UseSerilog();
    }
}