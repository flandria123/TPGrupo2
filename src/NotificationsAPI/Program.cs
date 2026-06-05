using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using HealthChecks.UI.Client;
using NotificationsAPI.Data;
using NotificationsAPI.Extensions;
using Dapper;

namespace NotificationsAPI;

public partial class Program
{
    public static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        // 1. CONFIGURACIÓN DE LOGGING (Serilog)
        // Mantiene la terminal limpia y guarda auditoría en archivo 
        builder.AddAppLogging();

        // 2. REGISTRO DE SERVICIOS (La magia de tu ServicesExtensions)
        // Aquí adentro se cargan Controllers, Swagger, DI, HttpClient, Handlers y HealthChecks 
        builder.Services.AddAppServices();

        // Configuración indispensable para que Dapper entienda los campos Guid en SQLite
        SqlMapper.AddTypeHandler(new GuidTypeHandler());

        var app = builder.Build();

        // 3. INICIALIZACIÓN (Después del Build)
        using (var scope = app.Services.CreateScope())
        {
            var services = scope.ServiceProvider;

            // Crea la tabla de notificaciones si no existe (Plug & Play) 
            services.GetRequiredService<DatabaseInitializer>().Initialize();

            // Log de confirmación de arranque 
            var logger = services.GetRequiredService<ILogger<Program>>();
            logger.LogInformation("Base de datos de Notificaciones inicializada y API lista.");
        }

        // 4. PIPELINE HTTP (Middlewares y Endpoints)
        if (app.Environment.IsDevelopment())
        {
            app.UseSwagger();   // expone el JSON en /swagger/v1/swagger.json 
            app.UseSwaggerUI(); // expone la UI en /swagger 
        }

        app.UseAppMiddleware(); // Correlation ID y Request Logging de Serilog 

        // Atrapa excepciones de dominio y emite códigos NTF-XXX 
        app.UseExceptionHandler();

        app.UseHttpsRedirection();
        app.UseAuthorization();

        // ── Mapeo de HealthChecks a la vista (Estilo Profesor)

        // Health Check general (Database + API)
        app.MapHealthChecks("/health", new HealthCheckOptions
        {
            ResponseWriter = UIResponseWriter.WriteHealthCheckUIResponse
        });

        // Liveness (solo que la API responda)
        app.MapHealthChecks("/health/live", new HealthCheckOptions
        {
            Predicate = r => r.Tags.Contains("api"),
            ResponseWriter = UIResponseWriter.WriteHealthCheckUIResponse
        });

        // Readiness (que la base de datos de SQLite esté lista)
        app.MapHealthChecks("/health/ready", new HealthCheckOptions
        {
            Predicate = r => r.Tags.Contains("database"),
            ResponseWriter = UIResponseWriter.WriteHealthCheckUIResponse
        });

        app.MapControllers();  // Activa tus controladores REST de Notifications.API

        app.Run();
    }
}