using CartAPI.Data;
using CartAPI.Extensions;
using Dapper;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Diagnostics;
using CartAPI.ExceptionHandlers;




public partial class Program
{
    private static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        // ─── 1. CONFIGURACIÓN DE SERVICIOS  ───

        // Guid Type
        SqlMapper.AddTypeHandler(new GuidTypeHandler());

        // 1. Logging 
        builder.AddAppLogging();

        // 2. Servicios de la aplicación 
        builder.Services.AddAppServices();

        // Agregar Swagger 
        builder.Services.AddEndpointsApiExplorer();
        builder.Services.AddSwaggerGen();


        var app = builder.Build();

        // ─── 2. INICIALIZACIÓN ───

        // Inicializar la Base de Datos de Carro 
        using (var scope = app.Services.CreateScope())
        {
            var services = scope.ServiceProvider;

            services.GetRequiredService<DatabaseInitializer>().Initialize();
        }

        // ─── 3. PIPELINE DE MIDDLEWARES Y RUTAS ───

        // Activar Swagger solo en desarrollo
        if (app.Environment.IsDevelopment())
        {
            app.UseSwagger();
            app.UseSwaggerUI();
        }


        app.UseAppMiddleware();
        app.UseExceptionHandler();

        // 5. Endpoints
        app.MapControllers();

        // Mapear Health Checks  
        app.MapHealthChecks("/health", new Microsoft.AspNetCore.Diagnostics.HealthChecks.HealthCheckOptions
        {
            ResponseWriter = HealthChecks.UI.Client.UIResponseWriter.WriteHealthCheckUIResponse
        });


        app.MapHealthChecks("/health/live", new Microsoft.AspNetCore.Diagnostics.HealthChecks.HealthCheckOptions
        {
            Predicate = r => r.Tags.Contains("api"),
            ResponseWriter = HealthChecks.UI.Client.UIResponseWriter.WriteHealthCheckUIResponse
        });


        // Health Check de Readiness (que la base de datos esté lista)
        app.MapHealthChecks("/health/ready", new Microsoft.AspNetCore.Diagnostics.HealthChecks.HealthCheckOptions
        {
            Predicate = r => r.Tags.Contains("database"),
            ResponseWriter = HealthChecks.UI.Client.UIResponseWriter.WriteHealthCheckUIResponse
        });

        app.Run();
    }
}