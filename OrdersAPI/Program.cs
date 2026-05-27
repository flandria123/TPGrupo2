using OrdersAPI.ExceptionHandlers;
using OrdersAPI.Services;
using Serilog;

// ============================================================================
// 1. CONFIGURACIÓN INICIAL DE SERILOG (Logs en archivo JSON)
// ============================================================================
Log.Logger = new LoggerConfiguration()
    .WriteTo.Console() // Muestra los logs en la consolita negra de Visual Studio
    .WriteTo.File(
        path: "Logs/orders-api-log-.json",
        rollingInterval: RollingInterval.Day, // Crea un archivo nuevo por día
        formatter: new Serilog.Formatting.Json.JsonFormatter()) // Lo guarda en formato JSON
    .CreateLogger();

try
{
    Log.Information("Iniciando la API de Órdenes...");
    var builder = WebApplication.CreateBuilder(args);

    // Reemplazamos el logger por defecto de .NET por nuestro Serilog
    builder.Host.UseSerilog();

    // ============================================================================
    // 2. REGISTRO DE SERVICIOS (Dependency Injection)
    // ============================================================================
    builder.Services.AddControllers();
    builder.Services.AddEndpointsApiExplorer();
    builder.Services.AddSwaggerGen();

    // A. Inyección de tu Servicio Principal
    builder.Services.AddScoped<IOrderService, OrderService>();

    // B. Configuración de HttpClientFactory (Comunicación entre microservicios)
    builder.Services.AddHttpClient("UsersAPI", client =>
    {
        client.BaseAddress = new Uri("https://localhost:7001"); // Ajustar puerto del TP
    });
    builder.Services.AddHttpClient("ProductsAPI", client =>
    {
        client.BaseAddress = new Uri("https://localhost:7002"); // Ajustar puerto del TP
    });

    // C. Manejo de Excepciones (El orden es vital: de lo más específico a lo global)
    builder.Services.AddExceptionHandler<ValidationExceptionHandler>();
    builder.Services.AddExceptionHandler<NotFoundExceptionHandler>();
    builder.Services.AddExceptionHandler<BusinessRuleExceptionHandler>();
    builder.Services.AddExceptionHandler<GlobalExceptionHandler>();
    builder.Services.AddProblemDetails();

    // D. ESPACIOS RESERVADOS PARA TUS PRÓXIMAS TAREAS (Data y HealthChecks)
    // TODO: Registrar tu DbContext y el Repositorio de la cátedra acá
    // builder.Services.AddDbContext<OrdersDbContext>(...);
    // builder.Services.AddScoped<IOrderRepository, OrderRepository>();

    // TODO: Configurar los HealthChecks acá
    // builder.Services.AddHealthChecks()...;

    // ============================================================================
    // 3. CONFIGURACIÓN DEL PIPELINE HTTP (Middlewares)
    // ============================================================================
    var app = builder.Build();

    // Activar el pipeline de intercepción de errores de .NET (Esto hace que los Handlers funcionen)
    app.UseExceptionHandler();

    if (app.Environment.IsDevelopment())
    {
        app.UseSwagger();
        app.UseSwaggerUI();
    }

    app.UseHttpsRedirection();

    // TODO: Agregar tus Middlewares personalizados acá (antes de UseAuthorization)
    // app.UseMiddleware<TuMiddlewarePersonalizado>();

    app.UseAuthorization();
    app.MapControllers();

    // TODO: Mapear el endpoint de HealthChecks acá
    // app.MapHealthChecks("/health");

    app.Run();
}
catch (Exception ex)
{
    Log.Fatal(ex, "La aplicación falló al arrancar.");
}
finally
{
    Log.CloseAndFlush(); // Asegura que los logs se guarden antes de que el programa se cierre
}