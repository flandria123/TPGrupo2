using Microsoft.Extensions.DependencyInjection;
using Users.API.Data;
using Users.API.HealthCheck;
using Users.API.Services;

namespace Users.API.Extensions
{
    public static class ServicesExtensions
    {
        public static void AddAppServices(this IServiceCollection services)
        {
            // 1. Registro del Inicializador que ya creaste
            services.AddSingleton<DatabaseInitializer>();

            // 2. Registro de tu Repositorio de Usuarios (o Service)

            services.AddScoped<UserRepository>();

            // 3. Capa de Negocio (El contrato y su implementación) [3, 10]
            services.AddScoped<IUserService, UserService>();

            // 4. Documentación (Swagger)
            services.AddEndpointsApiExplorer();
            services.AddSwaggerGen();

            // 5. Configuración de Health Checks (Lo que viste en ambos documentos)
            services.AddHealthChecks()
                .AddCheck<SqliteHealthCheck>("sqlite-db", tags: ["database"])
                .AddCheck<ApiStatusCheck>("api-status", tags: ["api"]);

            // 6. El Dashboard Visual (Health Checks UI)
            services.AddHealthChecksUI(setup =>
            {
                setup.SetEvaluationTimeInSeconds(600); // Evalúa cada 10 minutos [3]
                setup.AddHealthCheckEndpoint("User-API", "/health");
            }).AddInMemoryStorage();
        }
    }
}