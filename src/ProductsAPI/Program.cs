using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Diagnostics;
using Ecommerce.App.Extensions;
using ProductsAPI.ExceptionHandlers;
using ProductsAPI.Extensions;


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

        
        app.UseExceptionHandler();

        // 4. Middlewares
       // app.UseAppMiddleware();

        // 5. Endpoints
        app.MapControllers();

        app.Run();
    }
}