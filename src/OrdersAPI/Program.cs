using Dapper;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using OrdersAPI.Data;
using Orders.API.Extensions;
using OrdersAPI.Extensions; // Ajustá si tu namespace es Orders.API.Extensions

public partial class Program
{
    public static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        // ====================================================================
        // 1. CONFIGURACIÓN DE SERVICIOS
        // ====================================================================

        // Registro del GuidTypeHandler de Dapper (Igual que en Users API)
        SqlMapper.AddTypeHandler(new GuidTypeHandler());

        // Logging estructurado con Serilog
        builder.AddAppLogging();

        // Agregar servicios al contenedor 
        // (Nota: AddControllers, Swagger, ExceptionHandlers, HttpClients y HealthChecks 
        // ya están configurados internamente en esta extensión)
        builder.Services.AddAppServices();

        var app = builder.Build();

        // ====================================================================
        // 2. INICIALIZACIÓN DE BASE DE DATOS
        // ====================================================================
        using (var scope = app.Services.CreateScope())
        {
            var services = scope.ServiceProvider;

            // Ejecuta la creación de tablas y datos semilla de SQLite
            services.GetRequiredService<DatabaseInitializer>().Initialize();

            // Log de confirmación de arranque
            var logger = services.GetRequiredService<ILogger<Program>>();
            logger.LogInformation("Base de datos de Orders inicializada y API lista.");
        }

        // ====================================================================
        // 3. CONFIGURACIÓN DEL PIPELINE HTTP (Middlewares)
        // ====================================================================

        // Configura el manejo de errores global, Swagger UI, Serilog Request Logging,
        // el Middleware de Auditoría, el de Correlation ID y los HealthChecks.
        app.UseAppMiddleware();

        app.UseHttpsRedirection();
        app.UseAuthorization();

        // ====================================================================
        // 4. ENDPOINTS
        // ====================================================================

        // Activa tus controladores de Orders API
        app.MapControllers();

        app.Run();
    }
}