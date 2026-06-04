using System.Reflection;
using Microsoft.AspNetCore.Mvc;
using ProductsAPI.Data;
using ProductsAPI.Services;
using ProductsAPI.ExceptionHandlers; 

namespace ProductsAPI.Extensions;

public static class ServicesExtensions
{
    public static IServiceCollection AddAppServices(this IServiceCollection services)
    {
        // 1. CONFIGURACIÓN DE CONTROLADORES Y VALIDACIÓN (PRD-002)
        services.AddControllers()
            .ConfigureApiBehaviorOptions(options =>
            {
                // Apagamos el filtro automático para que el ValidationExceptionHandler tome el control
                options.SuppressModelStateInvalidFilter = true;
            });

        // 2. DOCUMENTACIÓN SWAGGER / OPENAPI 
        services.AddEndpointsApiExplorer();
        services.AddSwaggerGen(options =>
        {
            options.SwaggerDoc("v1", new()
            {
                Title = "Products API - E-Commerce",
                Version = "v1",
                Description = "Microservicio de gestión de catálogo de productos."
            });

            
            var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
            var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
            options.IncludeXmlComments(xmlPath);
        });

        // 3. COMUNICACIÓN HTTP ENTRE MICROSERVICIOS 
        services.AddHttpClient("OrdersAPI", client =>
        {
            
            client.BaseAddress = new Uri("https://localhost:7003");
        });

        // 4. INYECCIÓN DE DEPENDENCIAS (Dapper y Service)
        services.AddScoped<ProductRepository>();
        services.AddScoped<IProductService, ProductService>();

        // 5. MANEJO GLOBAL DE ERRORES (Problem Details y Handlers) [cite: 44]
        services.AddProblemDetails();
         //services.AddExceptionHandler<ValidationExceptionHandler>();
        // services.AddExceptionHandler<NotFoundExceptionHandler>();
         services.AddExceptionHandler<BusinessRuleExceptionHandler>();
         services.AddExceptionHandler<GlobalExceptionHandler>();

        // 6. HEALTH CHECKS 
        services.AddHealthChecks()
            // .AddCheck<SqliteHealthCheck>("sqlite-db", tags: new[] { "database" })
            // .AddCheck<ApiStatusCheck>("api-status", tags: new[] { "api" })
            ;

        return services;
    }
}