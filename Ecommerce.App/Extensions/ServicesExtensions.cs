using Microsoft.OpenApi;
using Ecommerce.App.APIS.Product.Models;
using Ecommerce.App.APIS.Product.Services;

namespace Ecommerce.App.Extensions;

public static class ServicesExtensions
{
    public static void AddAppServices(this IServiceCollection services)
    {
        // Servicios de negocio
        services.AddScoped<IProductService, ProductService>();

        // Swagger
        services.AddEndpointsApiExplorer();
        services.AddSwaggerGen(c =>
        {
            c.SwaggerDoc("v1", new OpenApiInfo
            {
                Title = "Grupo 2 - E-commerce",
                Version = "v1",
                Description = "API de Productos para E-commerce de Tecnologia"
            });
        });

        // Health Checks
        services.AddHealthChecks()
                .AddCheck<ProductsHealthCheck>("products-api", tags: ["api"]);
    }
}