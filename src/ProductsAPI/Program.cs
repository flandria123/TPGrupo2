using Dapper;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Diagnostics;
using ProductsAPI.Data;
using ProductsAPI.ExceptionHandlers;
using ProductsAPI.Extensions;



public partial class Program
{
    private static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        // ─── 1. CONFIGURACIÓN DE SERVICIOS (Antes del Build) ───

        // Guid Type
        SqlMapper.AddTypeHandler(new GuidTypeHandler());

        // 1. Logging (Usa tu extensión) [cite: 132]
        builder.AddAppLogging();

        // 2. Servicios de la aplicación (Usa tu extensión) [cite: 135]
        builder.Services.AddAppServices();

        
        
        var app = builder.Build();

        // ─── 2. INICIALIZACIÓN ───

        // Inicializar la Base de Datos de Productos 
        using (var scope = app.Services.CreateScope())
        {
            var services = scope.ServiceProvider;
            
            services.GetRequiredService<DatabaseInitializer>().Initialize();
        }

        // ─── 3. PIPELINE DE MIDDLEWARES Y RUTAS ───

        // Activar Swagger solo en desarrollo [cite: 72]
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