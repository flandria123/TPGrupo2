using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Diagnostics;
using Ecommerce.App.Extensions;
using Ecommerce.App.Products.API.ExceptionHandlers;

public partial class Program
{
    private static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);
        var app = builder.Build();

        // 1. Logging
        builder.AddAppLogging();

        // 2. Servicios de la aplicación
        builder.Services.AddAppServices();

        // 3. Exception Handlers
        builder.Services.AddExceptionHandler<NotFoundExceptionHandler>();
        builder.Services.AddExceptionHandler<BusinessRuleExceptionHandler>();
        builder.Services.AddExceptionHandler<GlobalExceptionHandler>();
        builder.Services.AddProblemDetails();

        app.UseExceptionHandler();

        // 4. Middlewares
        app.UseAppMiddleware();

        // 5. Endpoints
        app.MapControllers();

        app.Run();
    }
}