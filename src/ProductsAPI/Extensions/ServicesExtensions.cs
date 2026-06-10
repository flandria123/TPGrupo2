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

        services.AddHttpClient("OrdersAPI", client =>
        {
            client.BaseAddress = new Uri("https://localhost:7003");
        });

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

        
        services.AddEndpointsApiExplorer();

        
        services.AddSwaggerGen(options =>
        {
            
            var xmlFile = $"{System.Reflection.Assembly.GetExecutingAssembly().GetName().Name}.xml";
            var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);

            
            options.IncludeXmlComments(xmlPath);
        });

        return services;
    }
}