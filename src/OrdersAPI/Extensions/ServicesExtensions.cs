using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.OpenApi;
using OrdersAPI.Data;
using Orders.API.HealthCheck;
using OrdersAPI.ExceptionHandlers;
using OrdersAPI.Services;
using System;
using System.IO;

namespace OrdersAPI.Extensions
{
    public static class ServicesExtensions
    {
        public static void AddAppServices(this IServiceCollection services)
        {
            // 1. SOPORTE PARA CONTROLADORES REST
            services.AddControllers()
            .ConfigureApiBehaviorOptions(options =>
            {
                // Apagamos el filtro automático para que el ValidationExceptionHandler tome el control
                options.SuppressModelStateInvalidFilter = true;
            });

            // 2. DOCUMENTACIÓN INTERACTIVA (Swagger con comentarios XML)
            services.AddEndpointsApiExplorer();
            services.AddSwaggerGen(options =>
            {
                options.SwaggerDoc("v1", new OpenApiInfo
                {
                    Title = "Orders API",
                    Version = "v1",
                    Description = "Microservicio para la gestión de órdenes del E-Commerce"
                });

                // Configuración para leer los comentarios de triple barra (///) de controladores y DTOs
                var xmlFile = $"{System.Reflection.Assembly.GetExecutingAssembly().GetName().Name}.xml";
                var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
                options.IncludeXmlComments(xmlPath);
            });

            // 3. CAPA DE PERSISTENCIA Y LÓGICA DE NEGOCIO (Inyección de Dependencias)
            services.AddSingleton<DatabaseInitializer>();
            services.AddScoped<OrderRepository>(); // Registrado como clase concreta directamente
            services.AddScoped<IOrderService, OrderService>();

            // 4. COMUNICACIÓN HTTP ENTRE MICROSERVICIOS
            // Configuración de los clientes para consultar stock y validar identidades
            services.AddHttpClient("UsersAPI", client =>
            {
                client.BaseAddress = new Uri("https://localhost:7002"); // Ajustar puerto del grupo
            });
            services.AddHttpClient("ProductsAPI", client =>
            {
                client.BaseAddress = new Uri("https://localhost:7001"); // Ajustar puerto del grupo
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
                setup.AddHealthCheckEndpoint("OrdersApi", "/health"); // Endpoint de origen de datos
            }).AddInMemoryStorage(); // Almacena el historial de estados en la memoria de la app
        }
    }
}