using Dapper;
using HealthChecks.UI.Client;
using Serilog;
using System.Reflection;
using Users.API.Data;
using Users.API.ExceptionHandlers;
using Users.API.Extensions;


public partial class Program
{
    public static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        // 1. CONFIGURACIÓN DE SERVICIOS
        // Configurar Serilog
       
        builder.Services.AddSwaggerGen(c =>
        {
            // Localiza el archivo XML generado por el proyecto
            var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
            var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);

            // Indica a Swagger que use dicho archivo para las descripciones
            c.IncludeXmlComments(xmlPath);
        });

        builder.AddAppLogging();

               // Agregar servicios al contenedor
        builder.Services.AddAppServices();
        builder.Services.AddControllers();

        //REGISTRO DE ERRORES
        builder.Services.AddExceptionHandler<ValidationExceptionHandler>(); // Agregado para USR-002
        builder.Services.AddExceptionHandler<NotFoundExceptionHandler>();
        builder.Services.AddExceptionHandler<BusinessRuleExceptionHandler>();
        builder.Services.AddExceptionHandler<GlobalExceptionHandler>();
        builder.Services.AddProblemDetails();

        // Para el GuidTypeHandler de Dapper
        SqlMapper.AddTypeHandler(new GuidTypeHandler());

        var app = builder.Build();

        // 2. INICIALIZACIÓN (Después del Build)
        // Inicializar BD
        using (var scope = app.Services.CreateScope())
        {
            var services = scope.ServiceProvider;
            // Inicializar BD
            services.GetRequiredService<DatabaseInitializer>().Initialize();
            // Log de confirmación
            var logger = services.GetRequiredService<ILogger<Program>>();
            logger.LogInformation("Base de datos inicializada y API lista.");
        }



        //Habilitación en el pipeline
        if (app.Environment.IsDevelopment())
        {
            app.UseSwagger();   // expone el JSON en /swagger/v1/swagger.json 
            app.UseSwaggerUI(); // expone la UI en /swagger 
        }

        //Registrar Handler de Excepciones //app.MapAppEndpoints(); NO VA
        app.UseAppMiddleware();
        app.UseExceptionHandler();
        app.UseHttpsRedirection();
        app.UseAuthorization();

        // Mapeo de HealthChecks ------------------------------------

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

        app.MapControllers();  // Activa tus controladores de la Users.API
        app.Run();


    }
}
