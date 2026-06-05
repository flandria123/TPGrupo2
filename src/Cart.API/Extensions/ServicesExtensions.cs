using CartAPI.Data;
using CartAPI.ExceptionHandlers;
using CartAPI.HealthChecks;
using CartAPI.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.OpenApi;
using System.Reflection;

namespace CartAPI.Extensions;

public static class ServicesExtensions
{
    public static void AddAppServices(this IServiceCollection services)
    {
        // 1. SOPORTE PARA CONTROLADORES REST
        services.AddControllers();

        // (Agregado crucial) Apagar validación automática de ASP.NET para usar nuestro 
        // ValidationExceptionHandler personalizado y emitir los códigos de error del dominio (ej. CRT-002)
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
                Title = "Cart API",
                Version = "v1",
                Description = "Microservicio para la gestión del carrito de compras del E-Commerce"
            });

            // Configuración para leer los comentarios de triple barra (///) de controladores y DTOs
            var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
            var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
            options.IncludeXmlComments(xmlPath);
        });

        // 3. CAPA DE PERSISTENCIA Y LÓGICA DE NEGOCIO (Inyección de Dependencias)
        services.AddSingleton<DatabaseInitializer>();
        services.AddScoped<CartRepository>(); // Registrado como clase concreta directamente
        services.AddScoped<ICartService, CartService>();

        // 4. COMUNICACIÓN HTTP ENTRE MICROSERVICIOS
        // Configuración del cliente para validar la existencia y el stock contra Products API
        services.AddHttpClient("ProductsAPI", client =>
        {
            client.BaseAddress = new Uri("https://localhost:7001"); // Puerto explícito de Products.API
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
            setup.AddHealthCheckEndpoint("CartApi", "/health"); // Endpoint de origen de datos ajustado
        }).AddInMemoryStorage(); // Almacena el historial de estados en la memoria de la app
    }
}