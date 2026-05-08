using HealthChecks.UI.Client;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Serilog;

namespace Ecommerce.App.Extensions;

public static class MiddlewareExtensions
{
    public static void UseAppMiddleware(this WebApplication app)
    {
        if (app.Environment.IsDevelopment())
        {
            app.UseSwagger();
            app.UseSwaggerUI();
        }

        app.UseSerilogRequestLogging(options =>
        {
            options.GetLevel = (ctx, _, ex) => ex != null
                ? LogEventLevel.Error
                : LogEventLevel.Information;
        });

        // Exception Handler (importante)
        app.UseExceptionHandler();

        // Health Checks
        app.MapHealthChecks("/health", new HealthCheckOptions
        {
            ResponseWriter = UIResponseWriter.WriteHealthCheckUIResponse
        });

        app.MapHealthChecks("/health/ready");
        app.MapHealthChecks("/health/live");
    }
}