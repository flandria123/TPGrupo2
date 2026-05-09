using Serilog;
using Users.API.Data;
using Users.API.Extensions;
using HealthChecks.UI.Client;


public partial class Program
{
    public static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);
        
        // Configurar Serilog
        builder.AddAppLogging();
       
        // Agregar servicios al contenedor
        builder.Services.AddAppServices();
       
        var app = builder.Build();
        
        // Inicializar BD
           using (var scope = app.Services.CreateScope())
           scope.ServiceProvider.GetRequiredService<ILogger<Program>>();
     


        //Llamo a la BD
        using (var scope = app.Services.CreateScope())
            scope.ServiceProvider
                .GetRequiredService<DatabaseInitializer>()
                .Initialize();
        
        //Log de ejemplo
        var logger = app.Services.GetRequiredService<ILogger<Program>>();
        logger.LogError("OK");




        //Habilitación en el pipeline
        if (app.Environment.IsDevelopment())
        {
            app.UseSwagger();   // expone el JSON en /swagger/v1/swagger.json 
            app.UseSwaggerUI(); // expone la UI en /swagger 
        }

        // Mapeo de HealthChecks

        // Health Check general (Database + API)
        app.MapHealthChecks("/health", new Microsoft.AspNetCore.Diagnostics.HealthChecks.HealthCheckOptions
        {
            // Ahora debería reconocer UIResponseWriter directamente
            ResponseWriter = UIResponseWriter.WriteHealthCheckUIResponse
        });

        // Health Check de Liveness (solo que la API responda)
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


        


        app.UseAppMiddleware();
        //app.MapAppEndpoints();
        app.UseHttpsRedirection();
        app.UseAuthorization();
        app.MapControllers();
        app.Run();
    }
}
