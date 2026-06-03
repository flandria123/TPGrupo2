using Microsoft.AspNetCore.Mvc;
using Microsoft.OpenApi;
using NotificationsAPI.Data;
using NotificationsAPI.ExceptionHandlers;
using NotificationsAPI.HealthChecks;
using NotificationsAPI.Services;
using System.Reflection;

namespace NotificationsAPI.Extensions;

public static class ServicesExtensions
{
    public static void AddAppServices(this IServiceCollection services)
    {
        // 1. SOPORTE PARA CONTROLADORES REST
        services.AddControllers();

        // (Agregado crucial UBA) Apagar validación automática para usar nuestro 
        // ValidationExceptionHandler y emitir el código NTF-002
        services.Configure<ApiBehaviorOptions>(options =>
        {
            options.SuppressModelStateInvalidFilter = true;
        });

        // 2. DOCUMENTACIÓN INTERACTIVA (Swagger con comentarios XML)
        services.AddEndpointsApiExplorer();
        services.AddSwaggerGen(options =>
        {
            options.SwaggerDoc("v1", new OpenApiInfo
            {
                Title = "Notifications API",
                Version = "v1",
                Description = "Microservicio para el registro y simulación de envío de notificaciones del E-Commerce"
            });

            // Configuración para leer los comentarios de triple barra (///) de controladores y DTOs
            var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
            var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
            options.IncludeXmlComments(xmlPath);
        });

        // 3. CAPA DE PERSISTENCIA Y LÓGICA DE NEGOCIO (Inyección de Dependencias)
        // El profe lo registró como Singleton en su ejemplo
        services.AddSingleton<DatabaseInitializer>();
        services.AddScoped<NotificationsRepository>(); // Registrado como clase concreta directamente
        services.AddScoped<INotificationsService, NotificationsService>();

        // 4. COMUNICACIÓN HTTP ENTRE MICROSERVICIOS
        // Configuración del cliente para validar la existencia del usuario 
        services.AddHttpClient("UsersAPI", client =>
        {
            client.BaseAddress = new Uri("https://localhost:7001"); // Ajustar puerto de tu Users.API local
        });

       

        // 5. MANEJO GLOBAL DE EXCEPCIONES (IExceptionHandler + ProblemDetails)
        // El orden de registro determina la prioridad de evaluación
        services.AddExceptionHandler<ValidationExceptionHandler>();
        services.AddExceptionHandler<NotFoundExceptionHandler>();
        services.AddExceptionHandler<BusinessRuleExceptionHandler>();
        services.AddExceptionHandler<GlobalExceptionHandler>();
        services.AddProblemDetails();

        // 6. MONITOREO Y SALUD DEL MICROSERVICIO (Health Checks + Dashboard UI)
        services.AddHealthChecks()
            .AddCheck<SqliteHealthCheck>("sqlite-db", tags: ["ready", "database"])
            .AddCheck<ApiStatusCheck>("api-status", tags: ["live", "api"]);

        services.AddHealthChecksUI(setup =>
        {
            setup.SetEvaluationTimeInSeconds(600); // Evalúa la salud cada 10 minutos
            setup.AddHealthCheckEndpoint("NotificationsApi", "/health"); // Endpoint de origen de datos
        }).AddInMemoryStorage(); // Almacena el historial de estados en la memoria de la app
    }
}