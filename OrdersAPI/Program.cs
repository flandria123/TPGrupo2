using OrdersAPI.ExceptionHandlers;

var builder = WebApplication.CreateBuilder(args);

// --- REQUERIMIENTO 5.1: Documentación de APIs con Swagger ---
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// --- REQUERIMIENTO 5.2: Registro de Handlers (según Apéndice B) ---
builder.Services.AddExceptionHandler<NotFoundExceptionHandler>();
builder.Services.AddExceptionHandler<BusinessRuleExceptionHandler>();
builder.Services.AddProblemDetails(); // Formato estándar de errores

// --- REQUERIMIENTO 5.4: Health Checks ---
builder.Services.AddHealthChecks();

var app = builder.Build();

// --- REQUERIMIENTO 5.2: Activación del Manejo Global de Errores ---
// Esto hace que los Handlers que creaste funcionen en toda la API
app.UseExceptionHandler();

// --- REQUERIMIENTO 5.1: Interfaz de Swagger ---
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();

// --- REQUERIMIENTO 5.4: Endpoint de Monitoreo ---
app.MapHealthChecks("/health");

app.Run();