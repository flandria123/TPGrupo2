using ProductsAPI.Data; 
using ProductsAPI.Services;
using ProductsAPI.ExceptionHandlers;
using ProductsAPI.HealthChecks; 
using Microsoft.Extensions.DependencyInjection;

namespace ProductsAPI.Extensions;

public static class ServicesExtensions
{
    public static IServiceCollection AddAppServices(this IServiceCollection services)
    {
                       
         services.AddScoped<DatabaseInitializer>();

         services.AddHttpClient();

         services.AddScoped<ProductRepository>();
         services.AddScoped<IProductService, ProductService>();

         services.AddControllers()
            .ConfigureApiBehaviorOptions(options =>
            {
                
                options.SuppressModelStateInvalidFilter = true;
            });

                       
        services.AddExceptionHandler<ValidationExceptionHandler>(); 
        services.AddExceptionHandler<NotFoundExceptionHandler>();     
        services.AddExceptionHandler<BusinessRuleExceptionHandler>(); 
        services.AddExceptionHandler<GlobalExceptionHandler>();      
        services.AddProblemDetails();

              
        services.AddHealthChecks()
            .AddCheck<SqliteHealthCheck>("sqlite-db", tags: ["database"])
            .AddCheck<ApiStatusCheck>("api-status", tags: ["api"]);

        return services;
    }
}