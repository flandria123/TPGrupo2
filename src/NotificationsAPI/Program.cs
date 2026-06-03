using HealthChecks.UI.Client;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using NotificationsAPI.ExceptionHandlers;
using NotificationsAPI.Extensions;
using NotificationsAPI.HealthChecks;

public class Program
{
    public static void Main(string[] args)
    {

        var builder = WebApplication.CreateBuilder(args);

        // ─────────────────────────────────────────────
        // LOGGING (SERILOG)
        // ─────────────────────────────────────────────
        builder.AddAppLogging();

        // ─────────────────────────────────────────────
        // SERVICES
        // ─────────────────────────────────────────────
        builder.Services.AddControllers();

        builder.Services.AddEndpointsApiExplorer();

        builder.Services.AddSwaggerGen();

        // ─────────────────────────────────────────────
        // HEALTH CHECKS
        // ─────────────────────────────────────────────
        builder.Services.AddHealthChecks()

            // Estado general de la API
            .AddCheck<ApiStatusCheck>(
                "api-status",
                tags: new[] { "api" })

            // Estado de SQLite
            .AddCheck<SqliteHealthCheck>(
                "sqlite",
                tags: new[] { "database" });

        // ─────────────────────────────────────────────
        // EXCEPTION HANDLERS
        // ─────────────────────────────────────────────
        builder.Services.AddExceptionHandler<BusinessRuleExceptionHandler>();

        builder.Services.AddExceptionHandler<ValidationExceptionHandler>();

        builder.Services.AddExceptionHandler<NotFoundExceptionHandler>();

        builder.Services.AddExceptionHandler<GlobalExceptionHandler>();

        builder.Services.AddProblemDetails();

        var app = builder.Build();

        // ─────────────────────────────────────────────
        // SWAGGER
        // ─────────────────────────────────────────────
        if (app.Environment.IsDevelopment())
        {
            app.UseSwagger();

            app.UseSwaggerUI();
        }

        // ─────────────────────────────────────────────
        // EXCEPTION PIPELINE
        // ─────────────────────────────────────────────
        app.UseExceptionHandler();

        // ─────────────────────────────────────────────
        // CUSTOM MIDDLEWARE
        // ─────────────────────────────────────────────
        app.UseAppMiddleware();

        app.UseHttpsRedirection();

        app.UseAuthorization();

        // ─────────────────────────────────────────────
        // CONTROLLERS
        // ─────────────────────────────────────────────
        app.MapControllers();

        // ─────────────────────────────────────────────
        // HEALTH CHECKS
        // ─────────────────────────────────────────────

        // Health general (API + DB)
        app.MapHealthChecks("/health", new HealthCheckOptions
        {
            ResponseWriter = UIResponseWriter.WriteHealthCheckUIResponse
        });

        // Liveness → solo verificar que la API esté viva
        app.MapHealthChecks("/health/live", new HealthCheckOptions
        {
            Predicate = check => check.Tags.Contains("api"),
            ResponseWriter = UIResponseWriter.WriteHealthCheckUIResponse
        });

        // Readiness → verificar dependencias críticas
        app.MapHealthChecks("/health/ready", new HealthCheckOptions
        {
            Predicate = check => check.Tags.Contains("database"),
            ResponseWriter = UIResponseWriter.WriteHealthCheckUIResponse
        });

        app.Run();



    }
}