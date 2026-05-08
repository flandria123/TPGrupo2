using Users.API.Services.Extensions;
using Serilog;

public partial class Program
{
    public static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);
        
        // Configurar Serilog
        builder.AddAppLogging();
       
        // Agregar servicios al contenedor
        //builder.Services.AddAppServices();
       
        var app = builder.Build();
        
        // Configurar el pipeline de la aplicación
       using (var scope = app.Services.CreateScope())
        
            scope.ServiceProvider.GetRequiredService<ILogger<Program>>();
            //logger.LogError("OK");

        
        app.UseAppMiddleware();
       // app.MapAppEndpoints();
        app.UseHttpsRedirection();
        app.UseAuthorization();
        app.MapControllers();
        app.Run();
    }
}
